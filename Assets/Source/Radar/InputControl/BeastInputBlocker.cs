using Beast.Claw.GameCycle;
using Beast.Radar.UI;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.InputControl
{
    public class BeastInputBlocker : IBeastGameCycleListener
    {
        private readonly IBeastWindowManager _windowManager;
        private readonly IBeastCameraInputController _cameraInput;

        private int _blockCount;
        private bool _isBlock;

        public BeastInputBlocker(IBeastWindowManager windowManager, IBeastCameraInputController cameraInputController)
        {
            _windowManager = windowManager;
            _cameraInput = cameraInputController;
        }
        
        public void ListenGameCycle(IBeastGameCycleHandle handle)
        {
            handle.ListenOperation(this, BeastCycleEventID.Warmup, Warmup);
            handle.ListenOperation(this, BeastCycleEventID.Dispose, Dispose);
        }
        
        private UniTask<BeastInitializationResult> Warmup()
        {
            SubscribeWindowManager();

            return BeastInitializationResult.SuccessSync();
        }

        private UniTask<BeastInitializationResult> Dispose()
        {
            UnsubscribeWindowManager();
            
            return BeastInitializationResult.SuccessSync();
        }

        private void SubscribeWindowManager()
        {
            _windowManager.MouseOverUI += OnMouseOverUI;
            _windowManager.WindowChanged += OnWindowChanged;
        }

        private void UnsubscribeWindowManager()
        {
            _windowManager.MouseOverUI -= OnMouseOverUI;
            _windowManager.WindowChanged -= OnWindowChanged;
        }

        private void OnWindowChanged(bool isHasOpenedWindow)
        {
            if (isHasOpenedWindow)
            {
                _blockCount++;
            }
            else
            {
                _blockCount--;
            }

            RefreshBlockState();
        }

        private void OnMouseOverUI(bool over)
        {
            if (over)
            {
                _blockCount++;
            }
            else
            {
                _blockCount--;
            }
            
            RefreshBlockState();
        }

        private void RefreshBlockState()
        {
            if (_blockCount != 0 && !_isBlock)
            {
                _isBlock = true;
                _cameraInput.ChangeBlock(_isBlock);
            }
            else if (_blockCount == 0 && _isBlock)
            {
                _isBlock = false;
                _cameraInput.ChangeBlock(_isBlock);
            }
        }
    }
}