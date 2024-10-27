using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Radar.Cinematic;
using Beast.Radar.InputHandling;
using Beast.Radar.Player;
using Cysharp.Threading.Tasks;

namespace Beast.Radar
{
    public class PrefabLibrary : ScriptableConfig, IGameCycleListener
    {
        public CameraView cameraPrefab;
        public PlayerInputView playerInputPrefab;
        public PlayerView playerViewPrefab;
        public UIPrefabLibrary ui;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield return ui;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(ActionID.WARMUP, _warmup);
        }

        private async UniTask<ActionResult> _warmup(ActionContext context)
        {
            ActionResult result = await ui._warmup(context);
            
            if (result.status != EActionStatus.Success)
            {
                return result;
            }
            
            return ActionResult.Success();
        }
    }
}