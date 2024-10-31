using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Radar.Player;
using Beast.Radar.World;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

namespace Beast.Radar.UI
{
    public class HUDWindow : Controller, IWindow<Empty>, IGameCycleListener
    {
        private readonly CorePrefabLibrary _library;
        private readonly IWindowManager _windowManager;
        private readonly IWorldTime _worldTime;
        private readonly IPlayerController _playerController;
        private readonly PlayerPreferences _playerPreferences;
        private readonly UIConfig _uiConfig;

        private IntToStringConverter _intToString;
        private HUDView _view;
        private IWindowHandle<Empty> _pauseMenu;

        public HUDWindow(
            CorePrefabLibrary library,
            IWindowManager windowManager,
            IWorldTime worldTime,
            IPlayerController playerController,
            PlayerPreferences playerPreferences,
            UIConfig uiConfig)
        {
            _library = library;
            _windowManager = windowManager;
            _worldTime = worldTime;
            _playerController = playerController;
            _playerPreferences = playerPreferences;
            _uiConfig = uiConfig;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
        }

        public UniTask Open(Empty data)
        {
            _view.pathfindingImage.SetAlpha(_playerPreferences.isAutoPathfinding ? 0.5f : 0.2f);

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
            _worldTime.Changed += () =>
            {
                string time = string.Concat(
                    _intToString.ToString((int)_worldTime.GetHours()),
                    ":",
                    _intToString.ToString((int)_worldTime.GetMinutes()));
                _view.timeText.SetText(time);
            };

            _playerController.ChangeMovementState += () =>
            {
                _view.moveImage.SetSprite(_playerController.IsMovement
                    ? _uiConfig.onMoveButtonSprite
                    : _uiConfig.offMoveButtonSprite);
            };

            _view.pauseButton.Clicked += () => { _pauseMenu.Open(default); };

            _view.pathfindingButton.Clicked += () =>
            {
                _playerPreferences.isAutoPathfinding = !_playerPreferences.isAutoPathfinding;
                _view.pathfindingImage.SetAlpha(_playerPreferences.isAutoPathfinding ? 0.5f : 0.2f);
            };

            _view.moveButton.Clicked += () =>
            {
                _playerController.SwitchMovement();
            };
        }
        
    }
}