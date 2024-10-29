using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Radar.World;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public class HUDWindow : Controller, IWindow<Empty>, IGameCycleListener
    {
        private readonly IUIFactory _uiFactory;
        private readonly IWindowManager _windowManager;
        private readonly IWorldTime _worldTime;
        private readonly SelectablesController _selectablesController;

        private HUDView _view;
        private IWindowHandle<Empty> _pauseMenu;
        
        public HUDWindow(IUIFactory uiFactory, IWindowManager windowManager, IWorldTime worldTime)
        {
            _uiFactory = uiFactory;
            _windowManager = windowManager;
            _worldTime = worldTime;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(ActionID.WARMUP, _warmup);
            handle.ListenUpdate(ActionID.TICK, _tick);
            handle.ListenOperation(ActionID.DISPOSE, _dispose);
        }

        public UniTask Open(Empty data)
        {
           _setActive(true);

           return UniTask.CompletedTask;
        }

        public UniTask Close()
        {
            _setActive(false);

            return UniTask.CompletedTask;
        }

        private UniTask<ActionResult> _warmup(ActionContext context)
        {
            _uiFactory.Create(this, PrefabID.HudWindow, out _view);
                
            _windowManager.Register(this, WindowID.MainHUD);
            
            _view.transform.SetParent(_windowManager.CanvasTransform, false);

            _pauseMenu = _windowManager.GetHandle<Empty>(this, WindowID.Pause);
                
            _subscribeView();
            
            return ActionResult.SuccessSyncronous();
        }

        private void _tick(ActionContext obj)
        {
            _view.timeText.SetText($"{(int)_worldTime.GetHours():D2}:{(int)_worldTime.GetMinutes():D2}");
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
            _pauseMenu.Open(default);
        }

        private UniTask<ActionResult> _dispose(ActionContext context)
        {
            _unsubscribeView();

            return ActionResult.SuccessSyncronous();
        }
    }
}