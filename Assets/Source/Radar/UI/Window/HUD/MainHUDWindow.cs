using System;
using System.Globalization;
using Beast.Claw.Attributes;
using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Radar.Player;
using Beast.Radar.GameTime;
using Cysharp.Threading.Tasks;
using Noesis;
using Rect = UnityEngine.Rect;

namespace Beast.Radar.UI
{
    public class MainHUDWindow : Actor, IWindow<Empty>
    {
        [Required] private readonly PlayerPreference _preference;
        [Required] private readonly UIConfig _config;
        [Required] private readonly IPlayer _player;
        [Required] private readonly IWindowManager _windowManager;
        [Required] private readonly IGameTime _gameTime;
        [Required] private readonly IGraphyWindow _graphyWindow;
        
        private IntToStringConverter _intToString;
        private MainHUDView _view;

        private bool _isOpened;
        private DateTime _displayedDate;
        private Cache<(int, int)> _displayedDateCache;
        
        public UniTask Open(Empty data)
        {
            _fetchView();
            
            _view.Visibility = Visibility.Visible;

            _isOpened = true;

            _graphyWindow.Collapse();

            return UniTask.CompletedTask;
        }

        public UniTask Close()
        {
            _view.Visibility = Visibility.Collapsed;

            _isOpened = false;

            _graphyWindow.Expand();
            
            return UniTask.CompletedTask;
        }

        protected override UniTask _warmup(ConcurrentContext context)
        {
            _windowManager.Register(this, WindowID.HUD);

            _config.HUDWindowAsset.Instantiate(out _view);
            
            _windowManager.Attach(_view, 0);

            _view.Visibility = Visibility.Visible;

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

            if (_displayedDateCache.Update((now.Hour, now.Minute)))
            {
                _view.textTime.Text = now.ToString("t", CultureInfo.InvariantCulture);
            }
            
            _view.SetAutoPathFindingActive(_preference.isAutoPathfinding);

            _view.SetPlayerMovementState(_player.HasTarget, _player.IsMoving);
        }

        private void _subscribe()
        {
            _view.buttonSettings.Click += (_, _) =>
            {
                _windowManager.Open.Pause();
            };

            _view.autoPathFindingButton.Click += (_, _) =>
            {
                _preference.isAutoPathfinding = !_preference.isAutoPathfinding;
                
                _view.SetAutoPathFindingActive(_preference.isAutoPathfinding);
            };
       
            _view.buttonMove.Click += (_, _) =>
            {
                _player.ToggleMovementState();
            };
        }
    }
}