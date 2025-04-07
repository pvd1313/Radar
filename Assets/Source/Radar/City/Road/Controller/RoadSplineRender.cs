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

using TerrainData = Beast.Radar.Terrain.TerrainData;

namespace Beast.Radar.City
{
    public class RoadSplineRender : Actor, IRoadSplineRender
    {
        public RoadSplineMap RoadMap => _roadSplineMap;
        
        [Required] private readonly IWorldManager _worldManager;
        [Required] private readonly RoadSplineRenderConfig _config;
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
        private List<RoadSplineRenderLOD> _lods;

        protected override UniTask _warmup(ConcurrentContext context)
        {
            _gizmo = _gizmoManager.CreateHandle(this);

            _lods = new List<RoadSplineRenderLOD>();

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

            _roadSplineMap.Dispose();

            BProfiler.BeginSample("RoadSplineMap.Create");
            _roadSplineMap = RoadSplineMap.Create(_roadSpawner.RoadMap, Allocator.Persistent);
            BProfiler.EndSample();
            
            _disposeLODs();

            BProfiler.BeginSample("CreateLODs");
            for (int i = 0; i < _config.lods.Length; i++)
            {
                RoadSplineRenderLOD lod = new (terrain.terrainSize, i, _config.lods[i], _roadSplineMap);

                _lods.Add(lod);
            }
            BProfiler.EndSample();
            
            BDebug.Track(this, "SplinePointCount", _roadSplineMap.roadPoints.Length);
            BDebug.Track(this, "RoadCount", _roadSplineMap.roads.Length);
            
            return UniTask.CompletedTask;
        }

        protected override void _render(BlockingContext context)
        {
            RoadSplineRenderLOD activeLod = null;

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

            float lineWidth = _config.viewDistanceToPixelSizeCurve.Evaluate(cameraViewDistance);
            
            float alpha = 1f - BMath.InverseLerp(_config.alphaFadeTilePixelSize, _terrainTileRender.TilePixelSize);
            alpha = BMath.Clamp01(alpha);
            
            BDebug.Track(this, "LineWidth", lineWidth);
            BDebug.Track(this, "Alpha", alpha);

            RoadRenderMaterialSetup materialSetup = new()
            {
                lineWidth = lineWidth / _screen.Height(),
                texture = _config.texture,
                textureAlpha = alpha
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
            foreach (RoadSplineRenderLOD lod in _lods)
            {
                lod.Dispose();
            }
            
            _lods.Clear();
        }

        protected override void _dispose(BlockingContext obj)
        {
            _disposeLODs();
            
            BGraphic.TryDispose(ref _renderMaterial);
            
            _roadSplineMap.Dispose();
            _gizmo.Dispose();
        }
    }
}