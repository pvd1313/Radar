using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

namespace Beast.Radar.UI
{
    public class WorldViewInstance : Actor, IWorldViewInstance
    {
        public WorldView Instance { get; private set; }
        
        private readonly CorePrefabLibrary _prefabLibrary;
        
        public WorldViewInstance(CorePrefabLibrary prefabLibrary)
        {
            _prefabLibrary = prefabLibrary;
        }

        protected override UniTask _warmup(ConcurrentContext arg)
        {
            Instance = BScene.InstantiateMono(this, _prefabLibrary.worldView, float3.zero, quaternion.identity);

            return UniTask.CompletedTask;
        }
    }
}