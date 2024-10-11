using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Radar.Cinematic;
using Beast.Radar.GameCycle;
using Beast.Radar.InputControl;
using Cysharp.Threading.Tasks;

namespace Beast.Radar
{
    public class PrefabLibrary : Library, IGameCycleListener
    {
        public CameraView cameraPrefab;
        public PlayerInputView playerInputPrefab;
        public UIPrefabLibrary ui;
        
        public void ListenGameCycle(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Warmup, Warmup);
        }

        private async UniTask<OperationResult> Warmup()
        {
            OperationResult result = await ui.Initialize();
            
            if (result.status != EOperationStatus.Success)
            {
                return result;
            }
            
            return OperationResult.Success();
        }

        
    }
}