using Beast.Claw.Attributes;
using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

namespace Beast.Radar.UI
{
    public class WorldViewInstance : Actor, IWorldViewInstance
    {
        public WorldView View { get; private set; }
        
        [Required] private readonly CorePrefabLibrary _prefabLibrary;
        
        protected override UniTask _warmup(ConcurrentContext arg)
        {
            View = BScene.InstantiateMono(this, _prefabLibrary.worldView);

            return UniTask.CompletedTask;
        }
    }
}