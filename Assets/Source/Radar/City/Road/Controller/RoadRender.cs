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
        private Mesh _roadMeshLOD2;
        private Material _renderMaterial;
        private IGizmoHandle _gizmo;

        protected override UniTask _warmup(ConcurrentContext context)
        {
            _gizmo = _gizmoManager.CreateHandle(this);
            
            _roadMeshLOD2 = new Mesh();
            _roadMeshLOD2.name = nameof(RoadRender) + "_LOD2";
            _roadMeshLOD2.bounds = new Bounds(Vector3.zero, Vector3.one);

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
            
            _splineMap.Dispose();

            int maxRoadPointCount = 0;
            for (int i = 0; i < roadPathMap.roads.Length; i++)
            {
                maxRoadPointCount = BMath.Max(maxRoadPointCount, roadPathMap.roads[i].pointCount);
            }

            int roadCount = roadPathMap.roads.Length;

            RoadSplineMapJob splineMapJob = new()
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
            splineMapJob.Schedule(roadCount, 8).Complete();
            Profiler.EndSample();

            Profiler.BeginSample("RoadUtil.CreateRoadSplineMap");
            _splineMap = RoadUtil.CreateRoadSplineMap(roadPathMap, splineMapJob, maxRoadPointCount, Allocator.Persistent);
            Profiler.EndSample();
            
            Profiler.BeginSample("RoadMeshBuilder.BuildAllRoadMesh");
            RoadMeshBuilder.BuildAllRoadMesh(_splineMap, _roadMeshLOD2, _config.lod2SegmentLength, _gizmo);
            Profiler.EndSample();

            BDebug.Track(this, "SplinePointCount", _splineMap.roadPoints.Length);
            BDebug.Track(this, "RoadCount", _splineMap.roads.Length);
            BDebug.Track(this, "LOD2VertexCount", _roadMeshLOD2.vertexCount);
            BDebug.Track(this, "LOD2TriangleCount", _roadMeshLOD2.triangles.Length);

            //RoadDebug.DrawRoadSplineMap(_gizmo, _splineMap);
            if (_splineMap.roads.Length > 0)
            {
                RoadDebug.DrawRoadSpline(_gizmo, _splineMap, 0, 0.1f);
            }

            splineMapJob.outRoadSplineHandles.Dispose();
            splineMapJob.outRoadSplinePoints.Dispose();
            splineMapJob.outRoadCurveLengths.Dispose();
            splineMapJob.outRoadSplinePointCount.Dispose();
            splineMapJob.outRoadLength.Dispose();

            return UniTask.CompletedTask;
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
                mesh = _roadMeshLOD2,
            });
        }

        protected override void _dispose(BlockingContext obj)
        {
            BGraphic.TryDispose(ref _roadMeshLOD2);
            BGraphic.TryDispose(ref _renderMaterial);
            _splineMap.Dispose();
            _gizmo.Dispose();
        }
    }
}