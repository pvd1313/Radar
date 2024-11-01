using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Gizmos;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Beast.Radar.InputHandling;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public class TerrainBiomeTest : Controller, IGameCycleListener
    {
        private enum BiomeName
        {
            Empty = 0,
            Plains = 1,
            Coast = 2,
            Sand = 6,
            Shallow = 3,
            Deep = 4,
            Grass = 5,
            Forest = 7
        }
        
        private readonly IInputSystem _input;
        private readonly ICameraTransform _camera;
        private readonly ITerrainBackedBiome _biome;
        private readonly IGizmosManager _gizmos;
        
        private GizmosHandle _cell1kmGizmos;
        private GizmosHandle _cell8kmGizmos;

        public TerrainBiomeTest(
            IInputSystem input, 
            ICameraTransform camera, 
            ITerrainBackedBiome biome, 
            IGizmosManager gizmos)
        {
            _input = input;
            _camera = camera;
            _biome = biome;
            _gizmos = gizmos;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenBlocking(ActionID.RENDER, _render);
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
        }
        
        private UniTask _warmup(ConcurrentContext arg)
        {
            _cell1kmGizmos = _gizmos.GetHandle(this, "1kmCell", Color.red);
            _cell8kmGizmos = _gizmos.GetHandle(this, "8kmCell", Color.green);
            
            return UniTask.CompletedTask;
        }

        private void _render(BlockingContext context)
        {
            if (!_biome.IsReady)
            {
                return;
            }
            
            float2 worldPoint = _camera.ScreenToWorld(_input.GameInput.ScreenPoint.Value);
            
            // 1 km
            int2 worldPoint1km = BMath.FloorToInt(worldPoint);
            _cell1kmGizmos.DrawScreenDot(worldPoint1km);
            _cell1kmGizmos.DrawScreenDot(worldPoint1km + new float2(0, 1));
            _cell1kmGizmos.DrawScreenDot(worldPoint1km + new float2(1, 0));
            _cell1kmGizmos.DrawScreenDot(worldPoint1km + new float2(1, 1));
            
            BDebug.Track(this, "WorldPoint_1KM", worldPoint1km);
            
            byte biomeIndex = _biome.Terrain1kmCell.GetPixelClamped(worldPoint1km);
            
            BDebug.Track(this, "Biome_1KM", ((BiomeName) biomeIndex).ToString());
            
            // 8-16 km
            int cellSize = _biome.Terrain8kmCell.cellSize;
            BDebug.Track(this, "CellSize8km", cellSize);

            int2 worldPoint8km = BMath.FloorToInt(worldPoint / cellSize) * cellSize;
            _cell8kmGizmos.DrawScreenDot(worldPoint8km);
            _cell8kmGizmos.DrawScreenDot(worldPoint8km + new float2(0, cellSize));
            _cell8kmGizmos.DrawScreenDot(worldPoint8km + new float2(cellSize, 0));
            _cell8kmGizmos.DrawScreenDot(worldPoint8km + new float2(cellSize, cellSize));
            
            BDebug.Track(this, "WorldPoint_8KM", worldPoint8km);

            biomeIndex = _biome.Terrain8kmCell.GetPixelClamped(worldPoint1km);
            
            BDebug.Track(this, "Biome_8KM", ((BiomeName) biomeIndex).ToString());
        }
    }
}