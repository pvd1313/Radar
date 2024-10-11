using Beast.Claw.GameCycle;
using Beast.Radar.GameCycle;
using Beast.Radar.UI;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.InputControl
{
    public class InputBlocker : IGameCycleListener
    {
        private readonly IWindowManager _windowManager;
        private readonly ICameraInputController _cameraInput;

        private int _blockCount;
        private bool _isBlock;

        public InputBlocker(IWindowManager windowManager, ICameraInputController cameraInputController)
        {
            _windowManager = windowManager;
            _cameraInput = cameraInputController;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Warmup, Warmup);
            handle.ListenOperation(this, OperationID.Dispose, Dispose);
        }
        
        private UniTask<OperationResult> Warmup()
        {
            SubscribeWindowManager();

            return OperationResult.SuccessSync();
        }

        private UniTask<OperationResult> Dispose()
        {
            UnsubscribeWindowManager();
            
            return OperationResult.SuccessSync();
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