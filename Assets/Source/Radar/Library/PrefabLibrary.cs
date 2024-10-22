using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Radar.Cinematic;
using Beast.Radar.InputControl;
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
            handle.ListenOperation(OperationID.WARMUP, Warmup);
        }

        private async UniTask<OperationResult> Warmup()
        {
            OperationResult result = await ui._warmup();
            
            if (result.status != EOperationStatus.Success)
            {
                return result;
            }
            
            return OperationResult.Success();
        }
    }
}