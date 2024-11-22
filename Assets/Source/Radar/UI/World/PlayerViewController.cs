using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Beast.Claw.Text;
using Beast.Radar.Player;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public class PlayerViewController : Actor
    {
        private readonly ICameraTransform _camera;
        private readonly IPlayer _player;
        private readonly IWorldViewInstance _view;

        private Cache<bool> _playerHasTargetCache;
        private Cache<int> _remainingDistanceCache;
        private StringBuilder _stringBuilder;

        public PlayerViewController(IWorldViewInstance worldView, ICameraTransform camera, IPlayer player)
        {
            _view = worldView;
            _camera = camera;
            _player = player;
        }

        protected override UniTask _warmup(ConcurrentContext arg)
        {
            _view.View.target.gameObject.SetActive(false);
            _stringBuilder = new StringBuilder(64);
            
            return UniTask.CompletedTask;
        }

        protected override void _tick(BlockingContext blockingContext)
        {
            if (!_player.IsReady)
            {
                return;
            }

            _view.View.player.position = _camera.WorldToScreen(_player.Position).XY0();
            _view.View.target.position = _camera.WorldToScreen(_player.Target).XY0();

            if (_playerHasTargetCache.Update(_player.HasTarget))
            {
                _view.View.target.gameObject.SetActive(_playerHasTargetCache);
            }

            if (_remainingDistanceCache.Update((int)_player.GetRemainingDistanceToTarget()))
            {
                _stringBuilder.Clear();
                _stringBuilder.Add(_remainingDistanceCache);
                _stringBuilder.Add("km");

                _view.View.targetText.SetText(_stringBuilder.ToString());
            }
        }
    }
}