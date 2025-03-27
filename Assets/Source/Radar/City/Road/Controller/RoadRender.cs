using System.Collections.Generic;
using System.Diagnostics;
using Beast.Claw.Attributes;
using Beast.Claw.Collections;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Claw.Gizmo;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Beast.Radar.Terrain;
using Beast.Radar.World;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Profiler = UnityEngine.Profiling.Profiler;
using TerrainData = Beast.Radar.Terrain.TerrainData;

namespace Beast.Radar.City
{
    public class RoadRender : Actor, ITerrainBiomePostProcessor
    {
        [Required] private readonly IWorldManager _worldManager;
        [Required] private readonly RoadRenderConfig _config;
        [Required] private readonly IRoadSpawner _roadSpawner;
        [Required] private readonly IScreen _screen;
        [Required] private readonly ICameraTransform _camera;
        [Required] private readonly IGizmoManager _gizmoManager;
        [Required] private readonly ITerrainBiomeRender _terrainBiomeRender;
        [Required] private readonly ITerrainTileRender _terrainTileRender;
        
        private RoadSplineMap _roadSplineMap;
        private TerrainData _bakedForTerrain;
        private Mesh _roadMeshLOD2;
        private Material _renderMaterial;
        private IGizmoHandle _gizmo;
        private CellGrid<int> _roadGrid;
        private Dictionary<int2, Mesh> _roadGridMeshes;
        private CommandBuffer _biomeBuffer;
        private Material _biomeMaterial;

        protected override UniTask _warmup(ConcurrentContext context)
        {
            _gizmo = _gizmoManager.CreateHandle(this);

            _roadMeshLOD2 = BGraphic.CreateMesh(this);
            _roadMeshLOD2.name = "RoadRender_LOD2";
            _roadMeshLOD2.bounds = new Bounds(Vector3.zero, Vector3.one);
            _roadGrid = new CellGrid<int>(_config.lod1CellSize, 512, Allocator.Persistent);
            _roadGridMeshes = new Dictionary<int2, Mesh>();

            _terrainBiomeRender.RegisterPostProcessor(this, ETerrainBiomePostProcessorPriority.Road);

            _renderMaterial = BGraphic.CreateMaterial(this, _config.renderShader);
            
            
            _biomeMaterial = BGraphic.CreateMaterial(this, _config.renderShader);
            _biomeBuffer = new CommandBuffer();
            _biomeBuffer.name = "RoadRenderBiomeBuffer";
            
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
            
            _roadSplineMap.Dispose();

            Profiler.BeginSample("RoadSplineMap.Create");
            _roadSplineMap = RoadSplineMap.Create(roadPathMap, Allocator.Persistent);
            Profiler.EndSample();
            
            Profiler.BeginSample("RoadMeshBuilder.BuildAllRoadMesh");
            RoadMeshBuilder.BuildAllRoadMesh(
                _roadSplineMap, 
                _roadMeshLOD2, 
                _config.lod2SegmentLength,
                EPrecisionMode.Float);
            Profiler.EndSample();
            
            Profiler.BeginSample("ConstructRoadGrid");
            _roadGrid.Clear();
            for (int i = 0; i < roadPathMap.roads.Length; i++)
            {
                _roadGrid.AddItem(i, roadPathMap.roads[i].bounds);
            }
            Profiler.EndSample();

            _disposeGridMeshes();
            
            Profiler.BeginSample("RoadMeshBuilder.BuildRoadGridMesh");
            _roadGridMeshes = RoadMeshBuilder.BuildRoadGridMeshes(
                this,
                _roadSplineMap,
                "RoadRender_LOD1",
                _roadGrid,
                terrain.terrainSize,
                _config.lod1SegmentLength, 
                EPrecisionMode.Float);
            Profiler.EndSample();

            BDebug.Track(this, "SplinePointCount", _roadSplineMap.roadPoints.Length);
            BDebug.Track(this, "RoadCount", _roadSplineMap.roads.Length);
            BDebug.Track(this, "LOD2_Vertex", _roadMeshLOD2.vertexCount);
            BDebug.Track(this, "LOD1_MeshCount", _roadGridMeshes.Count);

            if (Build.IS_DEVELOPMENT && _roadGridMeshes.Count > 0)
            {
                int maxVertexCount = -1;
                int totalVertexCount = 0;

                foreach (Mesh mesh in _roadGridMeshes.Values)
                {
                    int vertexCount = mesh.vertexCount;

                    maxVertexCount = BMath.Max(maxVertexCount, vertexCount);

                    totalVertexCount += vertexCount;
                }
                
                BDebug.Track(this, "LOD1_Vertex_max", maxVertexCount);
                BDebug.Track(this, "LOD1_Vertex_total", totalVertexCount);
            }

            return UniTask.CompletedTask;
        }
        
        public void Render(TerrainBiomePostProcess task)
        {
            Profiler.BeginSample("RoadRender.Render(TerrainBiomePostProcess)");
            
            _biomeBuffer.Clear();
            _biomeBuffer.SetRenderTarget(task.biomeTexture);

            World2ClipMatrix matrix = new ()
            {
                screenSize = task.biomeTexture.Size(),
                cameraPosition = task.renderPosition,
                cameraViewDistance = task.viewDistance,
                cameraRotationRad = default,
                zOrder = RenderZOrder.TERRAIN_ROAD
            };

            RoadRenderMaterialSetup materialSetup = new()
            {
                shaderMode = ERoadShaderMode.Biome,
                biomeIndex = ETerrainBiome.Road,
                lineWidth = 1f / task.biomeTexture.height,
            };
            
            matrix.Apply(_renderMaterial);
            materialSetup.Apply(_renderMaterial);

            Bounds2D viewBounds = Bounds2D.FromPointWithRadius(task.renderPosition, task.viewDistance);
            
            _roadGrid.GetCellBounds(viewBounds, out int2 lowerBound, out int2 upperBound);
            for (int x = lowerBound.x; x <= upperBound.x; x++)
            for (int y = lowerBound.y; y <= upperBound.y; y++)
            {
                if (_roadGridMeshes.TryGetValue(new int2(x, y), out Mesh mesh))
                {
                    _biomeBuffer.DrawMesh(mesh, Matrix4x4.identity, _renderMaterial);
                }
            }

            BGraphic.ExecuteBuffer(_biomeBuffer);
            
            Profiler.EndSample();
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

            float distanceFactor = BMath.Clamp01(_camera.ViewDistance / _config.lod1ActivationDistance);
            float pixelSize = BMath.Lerp(_config.roadPixelSize, 1f - distanceFactor);

            RoadRenderMaterialSetup materialSetup = new()
            {
                lineWidth = pixelSize / _screen.Height(),
                texture = _config.texture,
                shaderMode = ERoadShaderMode.Texture,
                textureAlpha = 1f - _terrainTileRender.TileAlpha01
            };

            materialSetup.Apply(_renderMaterial);
            matrix.Apply(_renderMaterial);

            BDebug.Track(this, "pixelSize", pixelSize);
            
            if (_camera.ViewDistance > _config.lod1ActivationDistance)
            {
                _renderLOD2();
            }
            else
            {
                _renderLOD1();
            }
        }

        private void _renderLOD1()
        {
            _roadGrid.GetCellBounds(_camera.GetViewBounds(), out int2 lowerBound, out int2 upperBound);

            int vertexCount = 0;
            int meshCount = 0;
            
            for (int x = lowerBound.x; x <= upperBound.x; x++)
            for (int y = lowerBound.y; y <= upperBound.y; y++)
            {
                if (_roadGridMeshes.TryGetValue(new int2(x, y), out Mesh mesh))
                {
                    BGraphic.DrawMeshDelayed(new DrawMeshTask
                    {
                        material = _renderMaterial,
                        mesh = mesh
                    });

                    if (Build.IS_DEVELOPMENT)
                    {
                        meshCount++;
                        vertexCount += mesh.vertexCount;
                    }
                }
            }

            _debugTrackRender(1, meshCount, vertexCount);
        }

        private void _renderLOD2()
        {
            BGraphic.DrawMeshDelayed(new DrawMeshTask
            {
                material = _renderMaterial,
                mesh = _roadMeshLOD2,
            });

            _debugTrackRender(2, 1, _roadMeshLOD2.vertexCount);
        }

        [Conditional("DEVELOPMENT")]
        private void _debugTrackRender(int lodIndex, int meshCount, int vertexCount)
        {
            BDebug.Track(this, "Rendered_LOD", lodIndex);
            BDebug.Track(this, "Rendered_MeshCount", meshCount);
            BDebug.Track(this, "Rendered_VertexCount", vertexCount);
        }

        private void _disposeGridMeshes()
        {
            foreach (Mesh mesh in _roadGridMeshes.Values)
            {
                BGraphic.Dispose(mesh);
            }
            _roadGridMeshes.Clear();
        }

        protected override void _dispose(BlockingContext obj)
        {
            _disposeGridMeshes();
            
            BGraphic.TryDispose(ref _roadMeshLOD2);
            BGraphic.TryDispose(ref _renderMaterial);
            BGraphic.TryDispose(ref _biomeMaterial);
            
            _biomeBuffer.Dispose();
            
            _roadSplineMap.Dispose();
            _gizmo.Dispose();
            _roadGrid.Dispose();
        }
    }
}