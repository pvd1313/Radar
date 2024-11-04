using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Radar.GameTime;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

namespace Beast.Radar.UI
{
    public class HUDWindow : Controller, IWindow<Empty>, IGameCycleListener
    {
        private readonly CorePrefabLibrary _library;
        private readonly IWindowManager _windowManager;
        private readonly IGameTime _gameTime;
        private readonly SelectablesController _selectablesController;

        private HUDView _view;
        private IWindowHandle<Empty> _pauseMenu;
        
        public HUDWindow(CorePrefabLibrary library, IWindowManager windowManager, IGameTime gameTime)
        {
            _library = library;
            _windowManager = windowManager;
            _gameTime = gameTime;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup); 
            handle.ListenBlocking(ActionID.TICK, _tick);
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

        private UniTask _warmup(ConcurrentContext context)
        {
            _windowManager.Register(this, WindowID.MainHUD);
            
            _view = BScene.InstantiateMono(this, _library.hud, float3.zero, quaternion.identity);
            _view.transform.SetParent(_windowManager.CanvasTransform, false);
            _view.gameObject.SetActive(false);
            _view.pauseButton.Clicked += _onClickedPauseButton;

            _pauseMenu = _windowManager.GetHandle<Empty>(this, WindowID.Pause);
            
            return UniTask.CompletedTask;
        }

        private void _onClickedPauseButton()
        {
            _pauseMenu.Open(default);
        }
        
        private void _tick(BlockingContext context)
        {
            _view.timeText.SetText($"{(int)_gameTime.GetHours():D2}:{(int)_gameTime.GetMinutes():D2}");
        }
    }
}