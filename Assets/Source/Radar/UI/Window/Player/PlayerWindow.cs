using Beast.Claw.Attributes;
using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Beast.Claw.Text;
using Beast.Radar.Player;
using Cysharp.Threading.Tasks;
using Noesis;

namespace Beast.Radar.UI.Player
{
    public class PlayerWindow : Actor
    {
        [Required] private readonly ICameraTransform _camera;
        [Required] private readonly IPlayer _player;
        [Required] private readonly IWindowManager _windowManager;
        [Required] private readonly UIConfig _uiConfig;
        [Required] private IWorldElementCanvas _worldElement;

        private Cache<bool> _playerHasTargetCache;
        private Cache<int> _remainingDistanceCache;
        private StringBuilder _stringBuilder;
    
        private PlayerViewData _playerData;
        private PlayerView _playerView;

        private TargetViewData _targetData;
        private TargetView _targetView;
        
        protected override UniTask _warmup(ConcurrentContext arg)
        {
            _stringBuilder = new StringBuilder(64);

            _playerData = _uiConfig.playerViewData;
            _playerData.asset.Instantiate(out _playerView);
            _playerView.SetViewSize(_playerData.viewSize);
            _worldElement.AddElement(_playerView.view);

            _targetData = _uiConfig.targetViewData;
            _targetData.asset.Instantiate(out _targetView);
            _targetView.SetViewSize(_targetData.viewSize);
            _targetView.SetFontSize(_targetData.fontSize);
            _worldElement.AddElement(_targetView.view);
            
            _player.ChangedTarget += () =>
            {
                if (_player.Target.y < _player.Position.y)
                {
                    _worldElement.UpdateZIndex(_targetView.view, 1);
                    _worldElement.UpdateZIndex(_playerView.view, 0);
                }
                else
                {
                    _worldElement.UpdateZIndex(_targetView.view, 0);
                    _worldElement.UpdateZIndex(_playerView.view, 1);
                }
            };
            
            return UniTask.CompletedTask;
        }

        protected override void _tick(BlockingContext blockingContext)
        {
            if (!_player.IsReady)
            {
                return;
            }

            _worldElement.UpdatePosition(_playerView.view, _player.Position);
            _worldElement.UpdatePosition(_targetView.view, _player.Target);

            if (_playerHasTargetCache.Update(_player.HasTarget))
            {
                _targetView.view.Visibility = _player.HasTarget ? Visibility.Visible : Visibility.Collapsed;
            }
            
            if (_remainingDistanceCache.Update((int)_player.GetRemainingDistanceToTarget()))
            {
                _stringBuilder.Clear();
                _stringBuilder.Add(_remainingDistanceCache);
                _stringBuilder.Add("km");

                _targetView.distanceText.Text = _stringBuilder.ToString();
            }
        }
    }
}