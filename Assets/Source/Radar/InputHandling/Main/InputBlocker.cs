using Beast.Claw.GameCycle;
using Beast.Radar.UI;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.InputControl
{
    public class InputBlocker : IGameCycleListener
    {
        private readonly IWindowManager _windowManager;
        private readonly ICameraInputController _cameraInput;

        private bool _isBlockInput;
        private bool _isOverUI;
        private bool _isOpenWindow;
        
        public InputBlocker(IWindowManager windowManager, ICameraInputController cameraInputController)
        {
            _windowManager = windowManager;
            _cameraInput = cameraInputController;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(OperationID.WARMUP, _warmup);
            handle.ListenOperation(OperationID.DISPOSE, _dispose);
        }
        
        private UniTask<OperationResult> _warmup()
        {
            _subscribeWindowManager();

            return OperationResult.SuccessSync();
        }

        private UniTask<OperationResult> _dispose()
        {
            _unsubscribeWindowManager();
            
            return OperationResult.SuccessSync();
        }

        private void _subscribeWindowManager()
        {
            _windowManager.MouseOverUI += _onMouseOverUI;
            _windowManager.WindowChanged += _onWindowChanged;
        }

        private void _unsubscribeWindowManager()
        {
            _windowManager.MouseOverUI -= _onMouseOverUI;
            _windowManager.WindowChanged -= _onWindowChanged;
        }

        private void _onWindowChanged(bool isHasOpenedWindow)
        {
            _isOpenWindow = isHasOpenedWindow;
            _refreshBlockState();
        }

        private void _onMouseOverUI(bool over)
        {
            _isOverUI = over;
            _refreshBlockState();
        }

        private void _refreshBlockState()
        {
            bool isBlock = _isOpenWindow || _isOverUI;
            if (isBlock && !_isBlockInput)
            {
                _isBlockInput = true;
                _cameraInput.ChangeBlock(_isBlockInput);
            }
            else if (!isBlock && _isBlockInput)
            {
                _isBlockInput = false;
                _cameraInput.ChangeBlock(_isBlockInput);
            }
        }
    }
}