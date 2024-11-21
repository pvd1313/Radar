using System;
using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Radar.Player;
using Beast.Radar.GameTime;
using Cysharp.Threading.Tasks;
using Noesis;

namespace Beast.Radar.UI
{
    public class MainHUDWindow : Actor, IWindow<Empty>
    {
        private readonly PlayerPreferences _preferences;
        private readonly UIConfig _config;
        private readonly INoesisWindowManager _noesisWindow;
        private readonly IPlayerController _player;
        private readonly IWindowManager _windowManager;
        private readonly IGameTime _gameTime;
        
        private IntToStringConverter _intToString;
        private IWindowHandle<Empty> _pauseMenu;
        private MainHUDView _view;
        
        public MainHUDWindow(
            PlayerPreferences preferences,
            UIConfig config,
            INoesisWindowManager noesisWindow,
            IWindowManager windowManager,
            IPlayerController player,
            IGameTime gameTime)
        {
            _preferences = preferences;
            _config = config;
            _noesisWindow = noesisWindow;
            _windowManager = windowManager;
            _gameTime = gameTime;
            _player = player;
        }

        public UniTask Open(Empty data)
        {
            _view.ChangePathfindingState(_preferences.isAutoPathfinding);

            _view.Visibility = Visibility.Visible;

            return UniTask.CompletedTask;
        }

        public UniTask Close()
        {
            _view.Visibility = Visibility.Hidden;

            return UniTask.CompletedTask;
        }

        protected override UniTask _warmup(ConcurrentContext context)
        {
            _intToString = new IntToStringConverter(59, "D2");

            _windowManager.Register(this, WindowID.HUD);

            _config.HUDAsset.Instantiate(out _view);
            
            _noesisWindow.Attach(_view, 0);

            _view.Visibility = Visibility.Visible;

            _pauseMenu = _windowManager.GetHandle<Empty>(this, WindowID.PAUSE);
            
            _subscribe();

            return UniTask.CompletedTask;
        }

        private void _subscribe()
        {
            _gameTime.MinuteChanged += () =>
            {
                TimeSpan time = _gameTime.GetTime();
                
                _view.textTime.Text = $"{_intToString.Convert(time.Hours)}:{_intToString.Convert(time.Minutes)}";
            };

            _view.buttonSettings.Click += (sender, args) =>
            {
                _pauseMenu.Open(default);
            };

            _view.buttonPath.Click += (sender, args) =>
            {
                _preferences.isAutoPathfinding = !_preferences.isAutoPathfinding;
                
                _view.ChangePathfindingState(_preferences.isAutoPathfinding);
            };
            
            _player.ChangeMovementState += () =>
            {
                _view.ChangeMoveState(_player.IsMovement);
            };
       
            _view.buttonMove.Click += (sender, args) =>
            {
                _player.SwitchMovement();
            };
        }
    }
}