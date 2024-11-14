using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Gizmo;
using Beast.Claw.Graphic;
using Beast.Claw.Input;
using Beast.Claw.Mathematics;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public class TerrainBiomeTest : Controller, IGameCycleListener
    {
        private readonly ICameraTransform _camera;
        private readonly ITerrainBackedBiome _biome;
        private readonly IGizmoManager _gizmoManager;
        private readonly IInputDevice _input;

        private IGizmoHandle _gizmo;

        public TerrainBiomeTest(
            ICameraTransform camera, 
            ITerrainBackedBiome biome, 
            IGizmoManager gizmoManager,
            IInputDevice input)
        {
            _camera = camera;
            _biome = biome;
            _gizmoManager = gizmoManager;
            _input = input;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenBlocking(ActionID.RENDER, _render);
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
            handle.ListenBlocking(ActionID.DISPOSE, _dispose);
        }

        private UniTask _warmup(ConcurrentContext arg)
        {
            _gizmo = _gizmoManager.CreateHandle(this);
            
            return UniTask.CompletedTask;
        }

        private void _render(BlockingContext context)
        {
            if (!_biome.IsReady)
            {
                return;
            }
            
            _gizmo.Clear();

            if (_input.Drag.status == EAxisStatus.Inactive)
            {
                return;
            }
            
            float2 worldPoint = _camera.ViewportToWorld(_input.Drag.position);
            
            // 1 km
            int2 worldPoint1km = BMath.FloorToInt(worldPoint);
            _gizmo.SetColor(Color.red);
            _gizmo.AddPoint(worldPoint1km);
            _gizmo.AddPoint(worldPoint1km + new float2(0, 1));
            _gizmo.AddPoint(worldPoint1km + new float2(1, 0));
            _gizmo.AddPoint(worldPoint1km + new float2(1, 1));
            
            BDebug.Track(this, "WorldPoint_1KM", worldPoint1km);
            
            EBiomeName biomeIndex = _biome.Terrain1kmCell.GetPixelClamped(worldPoint1km);
            
            BDebug.Track(this, "Biome_1KM", biomeIndex.ToString());
            
            // 8-16 km
            int cellSize = _biome.Terrain8kmCell.cellSize;
            BDebug.Track(this, "CellSize8km", cellSize);
            
            int2 worldPoint8km = BMath.FloorToInt(worldPoint / cellSize) * cellSize;
            _gizmo.SetColor(Color.green);
            _gizmo.AddPoint(worldPoint8km);
            _gizmo.AddPoint(worldPoint8km + new float2(0, cellSize));
            _gizmo.AddPoint(worldPoint8km + new float2(cellSize, 0));
            _gizmo.AddPoint(worldPoint8km + new float2(cellSize, cellSize));
            
            BDebug.Track(this, "WorldPoint_8KM", worldPoint8km);
            
            biomeIndex = _biome.Terrain8kmCell.GetPixelClamped(worldPoint1km);
            
            BDebug.Track(this, "Biome_8KM", biomeIndex.ToString());
        }
        
        private void _dispose(BlockingContext context)
        {
            _gizmo.Dispose();
        }
    }
}