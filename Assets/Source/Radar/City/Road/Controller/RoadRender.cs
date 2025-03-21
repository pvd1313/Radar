using Beast.Claw.Attributes;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Claw.Gizmo;
using Beast.Claw.Graphic;
using Beast.Claw.Jobs;
using Beast.Claw.Math;
using Beast.Radar.World;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

using Profiler = UnityEngine.Profiling.Profiler;
using TerrainData = Beast.Radar.Terrain.TerrainData;

namespace Beast.Radar.City
{
    public class RoadRender : Actor
    {
        [Required] private readonly IWorldManager _worldManager;
        [Required] private readonly RoadRenderConfig _config;
        [Required] private readonly IRoadSpawner _roadSpawner;
        [Required] private readonly IScreen _screen;
        [Required] private readonly ICameraTransform _camera;
        [Required] private readonly IGizmoManager _gizmoManager;

        private RoadSplineMap _splineMap;
        private TerrainData _bakedForTerrain;
        private Mesh _roadMesh;
        private Material _renderMaterial;
        private IGizmoHandle _gizmo;

        protected override UniTask _warmup(ConcurrentContext context)
        {
            _gizmo = _gizmoManager.CreateHandle(this);
            
            _roadMesh = new Mesh();
            _roadMesh.name = nameof(RoadRender);
            _roadMesh.bounds = new Bounds(Vector3.zero, Vector3.one);

            _renderMaterial = BGraphic.CreateMaterial(this, _config.renderShader);
            
            return UniTask.CompletedTask;
        }

        protected override UniTask _gameLoad(ConcurrentContext context)
        {
            TerrainData terrain = _worldManager.GetCurrentWorld().terrain;

            if (_bakedForTerrain.IsSame(terrain))
            {
                return UniTask.CompletedTask;
            }
            
            _bakedForTerrain = terrain;

            RoadPathMap roadPathMap = _roadSpawner.RoadMap;

            Profiler.BeginSample("_buildRoadMesh");
            _buildRoadMesh(_roadSpawner.RoadMap, _roadMesh);
            Profiler.EndSample();
            
            _splineMap.Dispose();

            int maxRoadPointCount = 0;
            for (int i = 0; i < roadPathMap.roads.Length; i++)
            {
                maxRoadPointCount = BMath.Max(maxRoadPointCount, roadPathMap.roads[i].pointCount);
            }

            int roadCount = roadPathMap.roads.Length;

            RoadSplineJob splineJob = new()
            {
                inRoadPaths = roadPathMap.roads,
                inRoadPathPoints = roadPathMap.roadPoints,
                inMaxPointCountPerRoad = maxRoadPointCount,
                outRoadSplinePoints = BNative.TempJobArray<float2>(maxRoadPointCount * roadCount),
                outRoadSplineHandles = BNative.TempJobArray<float2>(maxRoadPointCount * roadCount),
                outRoadCurveLengths = BNative.TempJobArray<float>(maxRoadPointCount * roadCount),
                outRoadSplinePointCount = BNative.TempJobArray<int>(roadCount),
                outRoadLength = BNative.TempJobArray<float>(roadCount),
            };
            
            Profiler.BeginSample("RoadSplineJob");
            splineJob.Schedule(roadCount, 8).Complete();
            Profiler.EndSample();

            Profiler.BeginSample("RoadUtil.CreateRoadSplineMap");
            _splineMap = RoadUtil.CreateRoadSplineMap(roadPathMap, splineJob, maxRoadPointCount, Allocator.Persistent);
            Profiler.EndSample();

            BDebug.Track(this, "SplinePointCount", _splineMap.roadPoints.Length);
            BDebug.Track(this, "RoadCount", _splineMap.roads.Length);
            
            // RoadDebug.DrawRoadSplineMap(_gizmo, _splineMap);
            RoadDebug.DrawRoadSpline(_gizmo, _splineMap, 0, 0.1f);

            splineJob.outRoadSplineHandles.Dispose();
            splineJob.outRoadSplinePoints.Dispose();
            splineJob.outRoadCurveLengths.Dispose();
            splineJob.outRoadSplinePointCount.Dispose();
            splineJob.outRoadLength.Dispose();

            return UniTask.CompletedTask;
        }

        private static void _buildRoadMesh(RoadPathMap roadPathMap, Mesh roadMesh)
        {
            int vertexCount = 0;
            int triangleCount = 0;
            for (int i = 0; i < roadPathMap.roads.Length; i++)
            {
                RoadPath roadPath = roadPathMap.roads[i];
                
                RoadMeshBuilder.GetMeshSize(
                    roadPath.pointCount, 
                    ERoadMeshBuildingMode.SkipPoints, 
                    1, 
                    out int roadVertexCount,
                    out int roadTriangleCount);

                vertexCount += roadVertexCount;
                triangleCount += roadTriangleCount;
            }

            using NativeArray<float3> vertices = BNative.TempArray<float3>(vertexCount);
            using NativeArray<float2> uvs = BNative.TempArray<float2>(vertexCount);
            using NativeArray<float3> normals = BNative.TempArray<float3>(vertexCount); 
            using NativeArray<int> triangles = BNative.TempArray<int>(triangleCount * 3);

            vertexCount = 0;
            triangleCount = 0;
            
            for (int i = 0; i < roadPathMap.roads.Length; i++)
            {
                RoadPath roadPath = roadPathMap.roads[i];
                
                RoadMeshBuilder.GetMeshSize(
                    roadPath.pointCount, 
                    ERoadMeshBuildingMode.SkipPoints, 
                    1, 
                    out int roadVertexCount,
                    out int roadTriangleCount);
                
                RoadMeshBuilder.BuildMesh(new RoadMeshTask
                {
                    inPoints = roadPathMap.roadPoints.Slice(roadPath.startPointIndex, roadPath.pointCount),
                    inModeValue = 1,
                    inMode = ERoadMeshBuildingMode.SkipPoints,
                    inFirstVertexIndex = vertexCount,
                    outTriangleIndices = triangles.Slice(triangleCount * 3, roadTriangleCount * 3),
                    outVertices = vertices.Slice(vertexCount, roadVertexCount),
                    outUVs = uvs.Slice(vertexCount, roadVertexCount),
                    outNormals = normals.Slice(vertexCount, roadVertexCount)
                });

                vertexCount += roadVertexCount;
                triangleCount += roadTriangleCount;
            }
            
            roadMesh.Clear();
            roadMesh.SetVertices(vertices, 0, vertices.Length, MeshUpdateFlags.DontRecalculateBounds);
            roadMesh.SetNormals(normals);
            roadMesh.SetUVs(0, uvs);
            roadMesh.SetIndices(triangles, MeshTopology.Triangles, 0, false);
        }

        protected override void _render(BlockingContext context)
        {
            World2ClipMatrix matrix = new()
            {
                screenSize = _screen.Size,
                cameraPosition = _camera.Position,
                cameraViewDistance = _camera.ViewDistance,
                cameraRotationRad = _camera.RotationRad,
                zOrder = RenderZOrder.TERRAIN_ROAD
            };

            matrix.Apply(_renderMaterial);
            
            BGraphic.DrawMeshDelayed(new DrawMeshTask
            {
                material = _renderMaterial,
                matrix = float4x4.identity,
                mesh = _roadMesh,
            });
        }

        protected override void _dispose(BlockingContext obj)
        {
            BGraphic.TryDispose(ref _roadMesh);
            BGraphic.TryDispose(ref _renderMaterial);
            _splineMap.Dispose();
            _gizmo.Dispose();
        }
    }
}