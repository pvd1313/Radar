using Beast.Claw.Attributes;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Beast.Claw.Jobs;
using Beast.Radar.World;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
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
        
        private TerrainData _bakedForTerrain;
        private Mesh _roadMesh;
        private Material _renderMaterial;

        protected override UniTask _warmup(ConcurrentContext context)
        {
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

            RoadMap roadMap = _roadSpawner.RoadMap;

            int vertexCount = 0;
            int triangleCount = 0;
            for (int i = 0; i < roadMap.roads.Length; i++)
            {
                RoadInfo road = roadMap.roads[i];
                
                RoadMeshBuilder.GetMeshSize(
                    road.pointCount, 
                    ERoadMeshBuildingMode.SkipPoints, 
                    _config.lod0SkipPointsValue, 
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
            
            for (int i = 0; i < roadMap.roads.Length; i++)
            {
                RoadInfo road = roadMap.roads[i];
                
                RoadMeshBuilder.GetMeshSize(
                    road.pointCount, 
                    ERoadMeshBuildingMode.SkipPoints, 
                    _config.lod0SkipPointsValue, 
                    out int roadVertexCount,
                    out int roadTriangleCount);
                
                RoadMeshBuilder.BuildMesh(new RoadMeshTask
                {
                    inPoints = roadMap.roadPoints.Slice(road.startPointIndex, road.pointCount),
                    inModeValue = _config.lod0SkipPointsValue,
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
            
            _roadMesh.Clear();
            _roadMesh.SetVertices(vertices, 0, vertices.Length, MeshUpdateFlags.DontRecalculateBounds);
            _roadMesh.SetNormals(normals);
            _roadMesh.SetUVs(0, uvs);
            _roadMesh.SetIndices(triangles, MeshTopology.Triangles, 0, false);

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
                mesh = _roadMesh,
            });
        }

        protected override void _dispose(BlockingContext obj)
        {
            Object.Destroy(_roadMesh);
            BGraphic.TryDispose(ref _renderMaterial);
        }
    }
}