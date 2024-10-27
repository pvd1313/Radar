using Beast.Claw.GameCycle;
using Beast.Radar.UI;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.InputHandling
{
    public class InputBlocker : IGameCycleListener
    {
        private readonly IWindowManager _windowManager;
        private readonly IInputSystem _inputSystem;

        private bool _isBlockInput;
        private bool _isMouseOverUI;
        private bool _isHUDClosed;
        
        public InputBlocker(IWindowManager windowManager, IInputSystem inputSystem)
        {
            _windowManager = windowManager;
            _inputSystem = inputSystem;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(ActionID.WARMUP, _warmup);
            handle.ListenOperation(ActionID.DISPOSE, _dispose);
        }
        
        private UniTask<ActionResult> _warmup(ActionContext context)
        {
            _subscribeWindowManager();

            return ActionResult.SuccessSyncronous();
        }

        private UniTask<ActionResult> _dispose(ActionContext context)
        {
            _unsubscribeWindowManager();
            
            return ActionResult.SuccessSyncronous();
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

        private void _onWindowChanged(string newWindowID)
        {
            _isHUDClosed = newWindowID != WindowID.MainHUD;
            _refreshBlockState();
        }

        private void _onMouseOverUI(bool isMouseOverUI)
        {
            _isMouseOverUI = isMouseOverUI;
            _refreshBlockState();
        }

        private void _refreshBlockState()
        {
            bool isBlock = _isHUDClosed || _isMouseOverUI;
            if (isBlock && !_isBlockInput)
            {
                _isBlockInput = true;
                _inputSystem.GameInput.ChangeBlock(_isBlockInput);
            }
            else if (!isBlock && _isBlockInput)
            {
                _isBlockInput = false;
                _inputSystem.GameInput.ChangeBlock(_isBlockInput);
            }
        }
    }
}