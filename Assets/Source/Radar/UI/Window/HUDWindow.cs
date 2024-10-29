using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

namespace Beast.Radar.UI
{
    public class HUDWindow : Controller, IWindow<Empty>, IGameCycleListener
    {
        private readonly CorePrefabLibrary _library;
        private readonly IWindowManager _windowManager;
        private readonly SelectablesController _selectablesController;

        private HUDView _view;
        private IWindowHandle<Empty> _pauseMenu;
        
        public HUDWindow(CorePrefabLibrary library, IWindowManager windowManager)
        {
            _library = library;
            _windowManager = windowManager;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(ActionID.WARMUP, _warmup);
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

        private UniTask<ActionResult> _warmup(ConcurrentContext context)
        {
            _view = BScene.InstantiateMono(this, _library.hud, float3.zero, quaternion.identity);
            
            _windowManager.Register(this, WindowID.MainHUD);
            
            _view.transform.SetParent(_windowManager.CanvasTransform, false);

            _pauseMenu = _windowManager.GetHandle<Empty>(this, WindowID.Pause);
                
            _subscribeView();
            
            return ActionResult.SuccessSyncronous();
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

        private UniTask<ActionResult> _dispose(ConcurrentContext context)
        {
            _unsubscribeView();

            return ActionResult.SuccessSyncronous();
        }
    }
}