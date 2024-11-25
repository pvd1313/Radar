using System;
using System.Globalization;
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
        private readonly IPlayer _player;
        private readonly IWindowManager _windowManager;
        private readonly IGameTime _gameTime;
        
        private IntToStringConverter _intToString;
        private IWindowHandle<Empty> _pauseMenu;
        private MainHUDView _view;

        private bool _isOpened;
        private DateTime _displayedDate;
        
        public MainHUDWindow(
            PlayerPreferences preferences,
            UIConfig config,
            INoesisWindowManager noesisWindow,
            IWindowManager windowManager,
            IPlayer player,
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
            _fetchView();
            
            _view.SetAutoPathFindingState(_preferences.isAutoPathfinding);

            _view.Visibility = Visibility.Visible;

            _isOpened = true;

            return UniTask.CompletedTask;
        }

        public UniTask Close()
        {
            _view.Visibility = Visibility.Hidden;

            _isOpened = false;

            return UniTask.CompletedTask;
        }

        protected override UniTask _warmup(ConcurrentContext context)
        {
            _windowManager.Register(this, WindowID.HUD);

            _config.HUDWindowAsset.Instantiate(out _view);
            
            _noesisWindow.Attach(_view, 0);

            _view.Visibility = Visibility.Visible;

            _pauseMenu = _windowManager.GetHandle<Empty>(this, WindowID.PAUSE);
            
            _subscribe();

            return UniTask.CompletedTask;
        }

        protected override void _tick(BlockingContext context)
        {
            if (_isOpened)
            {
                _fetchView();
            }
        }

        private void _fetchView()
        {
            DateTime now = _gameTime.Now;
            
            bool hoursDiffer = _displayedDate.Hour != now.Hour;
            bool minutesDiffer = _displayedDate.Minute != now.Minute;

            if (hoursDiffer || minutesDiffer)
            {
                _displayedDate = now;

                _view.textTime.Text = now.ToString("t", CultureInfo.InvariantCulture);
            }
            
            _view.SetAutoPathFindingState(_preferences.isAutoPathfinding);

            _view.SetPlayerMovementState(_player.HasTarget, _player.IsMoving);
        }

        private void _subscribe()
        {
            _view.buttonSettings.Click += (_, _) =>
            {
                _pauseMenu.Open(default);
            };

            _view.autoPathFindingButton.Click += (_, _) =>
            {
                _preferences.isAutoPathfinding = !_preferences.isAutoPathfinding;
                
                _view.SetAutoPathFindingState(_preferences.isAutoPathfinding);
            };
       
            _view.buttonMove.Click += (_, _) =>
            {
                _player.ToggleMovementState();
            };
        }
    }
}