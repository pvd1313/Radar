using Beast.Claw.Attributes;
using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Beast.Claw.Text;
using Beast.Radar.Player;
using Cysharp.Threading.Tasks;
using Noesis;
using Screen = UnityEngine.Device.Screen;

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

            float scale = 1;
            
            if (Screen.dpi < 100)
            {
                float diagonalPixels = BMath.Sqrt(BMath.Pow(Screen.width, 2) + BMath.Pow(Screen.height, 2));
                
                float diagonalInches = diagonalPixels / Screen.dpi;

                scale = diagonalInches * 0.1f;
            }

            _playerData = _uiConfig.playerViewData;
            _playerData.asset.Instantiate(out _playerView);
            _playerView.SetViewSize(_playerData.viewSize * scale);
            _worldElement.AddElement(_playerView.player);

            _targetData = _uiConfig.targetViewData;
            _targetData.asset.Instantiate(out _targetView);
            _targetView.SetViewSize(_targetData.viewSize * scale);
            _targetView.SetFontSize(_targetData.fontSize * scale);
            _worldElement.AddElement(_targetView.target);

            _player.ChangedTarget += () =>
            {
                if (_player.Target.y < _player.Position.y)
                {
                    _worldElement.UpdateZIndex(_targetView.target, 1);
                    _worldElement.UpdateZIndex(_playerView.player, 0);
                }
                else
                {
                    _worldElement.UpdateZIndex(_targetView.target, 0);
                    _worldElement.UpdateZIndex(_playerView.player, 1);
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
            
            _worldElement.UpdatePosition(_playerView.player, _player.Position, _playerData.offset);
            _worldElement.UpdatePosition(_targetView.target, _player.Target, _targetData.offset);

            if (_playerHasTargetCache.Update(_player.HasTarget))
            {
                _targetView.target.Visibility = _player.HasTarget ? Visibility.Visible : Visibility.Collapsed;
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