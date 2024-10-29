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
        }

        public UniTask Open(Empty data)
        {
            _view.gameObject.SetActive(true);

           return UniTask.CompletedTask;
        }

        public UniTask Close()
        {
            _view.gameObject.SetActive(false);

            return UniTask.CompletedTask;
        }

        private UniTask<ActionResult> _warmup(ConcurrentContext context)
        {
            _windowManager.Register(this, WindowID.MainHUD);
            
            _view = BScene.InstantiateMono(this, _library.hud, float3.zero, quaternion.identity);
            _view.transform.SetParent(_windowManager.CanvasTransform, false);
            _view.gameObject.SetActive(false);
            _view.pauseButton.Clicked += _onClickedPauseButton;

            _pauseMenu = _windowManager.GetHandle<Empty>(this, WindowID.Pause);
            
            return ActionResult.SuccessSyncronous();
        }

        private void _onClickedPauseButton()
        {
            _pauseMenu.Open(default);
        }
    }
}