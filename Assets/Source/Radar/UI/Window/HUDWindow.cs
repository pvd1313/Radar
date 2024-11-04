using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Radar.Player;
using Beast.Radar.GameTime;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

namespace Beast.Radar.UI
{
    public class HUDWindow : Controller, IWindow<Empty>, IGameCycleListener
    {
        private readonly PlayerPreferences _preferences;
        private readonly CorePrefabLibrary _library;
        private readonly UIConfig _config;
        private readonly IPlayerController _player;
        private readonly IWindowManager _windowManager;
        private readonly IGameTime _gameTime;
        private readonly SelectablesController _selectablesController;

        private HUDView _view;
        private IWindowHandle<Empty> _pauseMenu;
        
        private IntToStringConverter _intToString;

        public HUDWindow(
            PlayerPreferences preferences,
            CorePrefabLibrary library,
            UIConfig config,
            IWindowManager windowManager,
            IPlayerController player,
            IGameTime gameTime)
        {
            _preferences = preferences;
            _library = library;
            _config = config;
            _windowManager = windowManager;
            _gameTime = gameTime;
            _player = player;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
        }

        public UniTask Open(Empty data)
        {
            _view.pathfindingImage.SetAlpha(_preferences.isAutoPathfinding ? 0.5f : 0.2f);

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
            _intToString = new IntToStringConverter(59, "D2");

            _windowManager.Register(this, WindowID.MainHUD);

            _view = BScene.InstantiateMono(this, _library.hud, float3.zero, quaternion.identity);
            _view.transform.SetParent(_windowManager.CanvasTransform, false);
            _view.gameObject.SetActive(false);

            _pauseMenu = _windowManager.GetHandle<Empty>(this, WindowID.Pause);

            _subscribe();

            return UniTask.CompletedTask;
        }

        private void _subscribe()
        {
            _gameTime.Changed += () =>
            {
                string time = string.Concat(
                    _intToString.ToString((int)_gameTime.GetHours()),
                    ":",
                    _intToString.ToString((int)_gameTime.GetMinutes()));
                _view.timeText.SetText(time);
            };

            _player.ChangeMovementState += () =>
            {
                _view.moveImage.SetSprite(_player.IsMovement
                    ? _config.offMoveButtonSprite
                    : _config.onMoveButtonSprite);
            };
           
            _view.moveButton.Clicked += () =>
            {
                _player.SwitchMovement();
            };

            _view.pauseButton.Clicked += () => { _pauseMenu.Open(default); };

            _view.pathfindingButton.Clicked += () =>
            {
                _preferences.isAutoPathfinding = !_preferences.isAutoPathfinding;
                _view.pathfindingImage.SetAlpha(_preferences.isAutoPathfinding ? 0.5f : 0.2f);
            };
        }
    }
}