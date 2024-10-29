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
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
        }
        
        private UniTask _warmup(ConcurrentContext context)
        {
            _windowManager.MouseOverUI += _onMouseOverUI;
            _windowManager.WindowChanged += _onWindowChanged;

            return UniTask.CompletedTask;
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