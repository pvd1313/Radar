using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Beast.Radar.Player;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public class PlayerViewController : Actor
    {
        private readonly ICameraTransform _camera;
        private readonly IPlayer _player;
        private readonly IWorldViewInstance _view;

        public PlayerViewController(IWorldViewInstance worldView, ICameraTransform camera, IPlayer player)
        {
            _view = worldView;
            _camera = camera;
            _player = player;
        }

        protected override UniTask _warmup(ConcurrentContext arg)
        {
            _view.View.target.gameObject.SetActive(false);
            
            return UniTask.CompletedTask;
        }

        protected override void _tick(BlockingContext blockingContext)
        {
            if (_player.IsReady)
            {
                _view.View.player.position = _camera.WorldToScreen(_player.Position).XY0();
                _view.View.target.position = _camera.WorldToScreen(_player.Target).XY0();
                
                _view.View.target.gameObject.SetActive(_player.HasTarget);
            }
        }

        private void _refreshTargetView()
        {
            //TODO: https://risenbeast.youtrack.cloud/issue/R-6 Set travel time 
            
            /*float distance = BMath.Distance(_player.Position, _player.Target) * 1000;

            string distanceText;

            if (distance < 1000)
            {
                distanceText = $"{distance:F0} м";
            }
            else
            {
                float distanceInKm = distance / 1000f;
                distanceText = $"{distanceInKm:F1} км";
            }

            _view.targetText.SetText($"{distanceText}\n{travelTime}");*/
        }
    }
}