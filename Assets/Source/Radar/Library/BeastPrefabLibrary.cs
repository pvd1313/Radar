using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Radar.InputControl;
using Cysharp.Threading.Tasks;

namespace Beast.Radar
{
    public class BeastPrefabLibrary : BeastLibrary, IBeastInitialize
    {
        public BeastPlayerInputView playerInputPrefab;

        public BeastUIPrefabLibrary ui;
        
        public async UniTask<BeastInitializationResult> Initialize()
        {
            await ui.Initialize();

            return BeastInitializationResult.Success();
        }
    }
}