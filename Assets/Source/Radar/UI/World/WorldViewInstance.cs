using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

namespace Beast.Radar.UI
{
    public class WorldViewInstance : Controller, IWorldViewInstance, IGameCycleListener
    {
        public WorldView Instance { get; private set; }
        
        private readonly CorePrefabLibrary _prefabLibrary;
        
        public WorldViewInstance(CorePrefabLibrary prefabLibrary)
        {
            _prefabLibrary = prefabLibrary;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);    
        }

        private UniTask _warmup(ConcurrentContext arg)
        {
            Instance = BScene.InstantiateMono(this, _prefabLibrary.worldView, float3.zero, quaternion.identity);

            return UniTask.CompletedTask;
        }
    }
}