using System.Collections.Generic;
using Beast.Claw.Attributes;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Claw.Gizmo;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Beast.Radar.Terrain;
using Beast.Radar.World;
using Cysharp.Threading.Tasks;
using Unity.Collections;
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
        private Material _renderMaterial;
        private IGizmoHandle _gizmo;
        private CommandBuffer _biomeBuffer;
        private Material _biomeMaterial;
        private List<RoadRenderLOD> _lods;

        protected override UniTask _warmup(ConcurrentContext context)
        {
            _gizmo = _gizmoManager.CreateHandle(this);

            _lods = new List<RoadRenderLOD>();
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

            _roadSplineMap.Dispose();

            Profiler.BeginSample("RoadSplineMap.Create");
            _roadSplineMap = RoadSplineMap.Create(_roadSpawner.RoadMap, Allocator.Persistent);
            Profiler.EndSample();
            
            _disposeLODs();

            Profiler.BeginSample("CreateLODs");
            for (int i = 0; i < _config.lods.Length; i++)
            {
                RoadRenderLOD lod = new (terrain.terrainSize, i, _config.lods[i], _roadSplineMap);

                _lods.Add(lod);
            }
            Profiler.EndSample();
            
            BDebug.Track(this, "SplinePointCount", _roadSplineMap.roadPoints.Length);
            BDebug.Track(this, "RoadCount", _roadSplineMap.roads.Length);

            return UniTask.CompletedTask;
        }
        
        public void Render(TerrainBiomePostProcess task)
        {
            Profiler.BeginSample("RoadRender.Render(TerrainBiomePostProcess)");

            if (_lods.Count == 0)
            {
                return;
            }
            
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
            
            matrix.Apply(_biomeMaterial);
            materialSetup.Apply(_biomeMaterial);

            Bounds2D viewBounds = Bounds2D.FromPointWithRadius(task.renderPosition, task.viewDistance);

            RoadRenderLODResult renderResult = _lods[0].RenderToCommandBuffer(_biomeBuffer, _biomeMaterial, viewBounds);

            BDebug.Track(this, "Biome_mesh_count", renderResult.renderedMeshCount);
            BDebug.Track(this, "Biome_vertex_count", renderResult.renderedVertexCount);

            BGraphic.ExecuteBuffer(_biomeBuffer);
            
            Profiler.EndSample();
        }

        protected override void _render(BlockingContext context)
        {
            RoadRenderLOD activeLod = null;

            float cameraViewDistance = _camera.ViewDistance;

            int activeLodIndex = 0;
            for (; activeLodIndex < _lods.Count; activeLodIndex++)
            {
                activeLod = _lods[activeLodIndex];

                if (activeLod.Config.isGlobalLOD || cameraViewDistance < activeLod.Config.activationDistance)
                {
                    break;
                }
            }

            if (activeLod == null)
            {
                return;
            }
            
            World2ClipMatrix matrix = new()
            {
                screenSize = _screen.Size,
                cameraPosition = _camera.Position,
                cameraViewDistance = cameraViewDistance,
                cameraRotationRad = _camera.RotationRad,
                zOrder = RenderZOrder.TERRAIN_ROAD
            };

            float pixelSize = _config.viewDistanceToPixelSizeCurve.Evaluate(cameraViewDistance);

            BDebug.Track(this, "PixelSize", pixelSize);

            RoadRenderMaterialSetup materialSetup = new()
            {
                lineWidth = pixelSize / _screen.Height(),
                texture = _config.texture,
                shaderMode = ERoadShaderMode.Texture,
                textureAlpha = 1f - _terrainTileRender.TileAlpha01
            };

            materialSetup.Apply(_renderMaterial);
            matrix.Apply(_renderMaterial);

            BDebug.Track(this, "LOD", activeLodIndex);
            
            RoadRenderLODResult renderResult = activeLod.RenderToCamera(_renderMaterial, _camera.GetViewBounds());
            
            BDebug.Track(this, "LOD_mesh_count", renderResult.renderedMeshCount);
            BDebug.Track(this, "LOD_vertex_count", renderResult.renderedVertexCount);
        }

        private void _disposeLODs()
        {
            foreach (RoadRenderLOD lod in _lods)
            {
                lod.Dispose();
            }
            
            _lods.Clear();
        }

        protected override void _dispose(BlockingContext obj)
        {
            _disposeLODs();
            
            BGraphic.TryDispose(ref _renderMaterial);
            BGraphic.TryDispose(ref _biomeMaterial);
            
            _biomeBuffer.Dispose();
            
            _roadSplineMap.Dispose();
            _gizmo.Dispose();
        }
    }
}