using Beast.Claw.GameCycle;
using Beast.Radar.GameCycle;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public class HUDWindow : IWindow, IGameCycleListener
    {
        private readonly IUIFactory _uiFactory;
        private readonly IWindowManager _windowManager;
        private readonly SelectablesController _selectablesController;

        private HUDView _view;
        
        public HUDWindow(IUIFactory uiFactory, IWindowManager windowManager)
        {
            _uiFactory = uiFactory;
            _windowManager = windowManager;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.WARMUP, _warmup);
            handle.ListenOperation(this, OperationID.DISPOSE, _dispose);
        }

        public void Open()
        {
           _setActive(true);
        }

        public void Close()
        {
            _setActive(false);
        }

        private UniTask<OperationResult> _warmup()
        {
            _uiFactory.Create(this, PrefabID.HudWindow, out _view);
                
            _windowManager.Register(this, WindowID.MainHUD, _view);
                
            _subscribeView();
            
            return OperationResult.SuccessSync();
        }

        private void _setActive(bool isActive)
        {
            _view.pauseButton.SetInteractable(isActive);
        }

        private void _subscribeView()
        {
            _view.pauseButton.Clicked += _onClickedPauseButton;
        }

        private void _unsubscribeView()
        {
            _view.pauseButton.Clicked -= _onClickedPauseButton;
        }

        private void _onClickedPauseButton()
        {
            _windowManager.Show(WindowID.Pause);
        }

        private UniTask<OperationResult> _dispose()
        {
            _unsubscribeView();

            return OperationResult.SuccessSync();
        }
    }
}