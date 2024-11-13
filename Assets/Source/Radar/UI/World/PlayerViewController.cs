using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Beast.Radar.Player;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public class PlayerViewController : Controller, IGameCycleListener 
    {
        private readonly ICameraTransform _camera;
        private readonly IPlayerController _player;
        private readonly IWorldViewInstance _view;

        public PlayerViewController(IWorldViewInstance worldView, ICameraTransform camera, IPlayerController player)
        {
            _view = worldView;
            _camera = camera;
            _player = player;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenBlocking(ActionID.TICK, _tick);
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
        }
        
        private UniTask _warmup(ConcurrentContext arg)
        {
            _view.Instance.target.gameObject.SetActive(false);
            
            _player.ChangeTarget += () =>
            {
                _refreshTargetView();
                
                if (_player.Target.y > _player.Position.y)
                {
                    _view.Instance.target.SetAsFirstSibling();
                }
                else
                {
                    _view.Instance.target.SetAsLastSibling();
                }
            };
            
            return UniTask.CompletedTask;
        }

        private void _tick(BlockingContext blockingContext)
        {
            if (_player.IsReady)
            {
                _view.Instance.player.position = _camera.WorldToScreen(_player.Position).XY0();
                _view.Instance.target.position = _camera.WorldToScreen(_player.Target).XY0();
                
                if (_player.IsMovement)
                {
                    _refreshTargetView();
                }
            }
        }

        private void _refreshTargetView()
        {
            bool isNear = _player.Position.Equals(_player.Target);
         
            if (_view.Instance.target.gameObject.activeSelf == isNear)
            {
                _view.Instance.target.gameObject.SetActive(!_view.Instance.target.gameObject.activeSelf);
            }
            
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