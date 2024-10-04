using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Radar.InputControl;
using Cysharp.Threading.Tasks;

namespace Beast.Radar
{
    public class BeastPrefabLibrary : BeastLibrary, Initializable
    {
        public PlayerInputView playerInputPrefab;

        public BeastUIPrefabLibrary ui;
        
        public async UniTask<InitializationResult> Initialize()
        {
            await ui.Initialize();

            return InitializationResult.Success();
        }
    }
}