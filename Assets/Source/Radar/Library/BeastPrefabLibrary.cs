using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Radar.InputControl;
using Cysharp.Threading.Tasks;

namespace Beast.Radar
{
    public class BeastPrefabLibrary : BeastLibrary, IBeastGameCycleListener
    {
        public BeastPlayerInputView playerInputPrefab;
        public BeastUIPrefabLibrary ui;
        
        public void ListenGameCycle(IBeastGameCycleHandle handle)
        {
            handle.ListenOperation(this, BeastCycleEventID.Warmup, Warmup);
        }

        private async UniTask<BeastInitializationResult> Warmup()
        {
            BeastInitializationResult result = await ui.Initialize();
            
            if (result.status != EBeastInitializationStatus.Success)
            {
                return result;
            }
            
            return BeastInitializationResult.Success();
        }

        
    }
}