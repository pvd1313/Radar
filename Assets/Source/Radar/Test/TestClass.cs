using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Gizmos;
using Beast.Radar.Player.Path;
using Beast.Radar.Terrain;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Test
{
    public class TestClass : Controller, IGameCycleListener, IGizmosDrawer
    {
        private readonly ITerrainBackedBiome _backedBiome;

        private int _lenght;
        private int _square;
        private NativeArray<int2> _path;
        private bool _flag;

        public TestClass(ITerrainBackedBiome backedBiome)
        {
            _backedBiome = backedBiome;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
            handle.ListenBlocking(ActionID.TICK, _tick);
            handle.ListenBlocking(ActionID.DISPOSE, _dispose);
        }

        private UniTask _warmup(ConcurrentContext arg)
        {
            return UniTask.CompletedTask;
        }

        private int2[] path;
        private  void _tick(BlockingContext arg)
        {
            if (_lenght > 0)
            {
                return;
            }

            if (_lenght == 0)
            {
                 _lenght = _backedBiome.Terrain8kmCell.texture.Length;
            }

            if (_lenght > 0)
            {
                _square = (int)math.sqrt(_lenght);

                if (!_flag)
                {
                    var pathfinding = new Pathfinding(_square);
                 
                   // path = pathfinding.FindPathArray(int2.zero, new int2(15, 45), _backedBiome.Terrain8kmCell.texture ,out long time);
                }
                
                if (path != null && path.Length > 0)
                {
                    BDebug.LogInfo(this, $"{_lenght} {_square}  {path.Length} time");
                }
            }
        }

        public Color GizmosColor { get; } = Color.red;

        public void Draw(GizmosHandle handle)
        {
            if (path == null || path.Length == 0)
            {
                return;
            }

            foreach (int2 point in path)
            {
                handle.DrawScreenDot(point);
            }
        }

        private void _dispose(BlockingContext arg)
        {
            _path.Dispose();
        }
    }
}