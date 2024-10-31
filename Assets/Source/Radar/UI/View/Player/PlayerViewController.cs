using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Beast.Radar.Player;
using Beast.Radar.World;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

namespace Beast.Radar.UI
{
    public class PlayerViewController : Controller, IGameCycleListener 
    {
        private readonly CorePrefabLibrary _library;
        private readonly ICameraTransform _camera;
        private readonly IWorldTime _time;
        private readonly IPlayerController _player;

        private PlayerView _view;

        public PlayerViewController(CorePrefabLibrary library, ICameraTransform camera, IWorldTime time, IPlayerController player)
        {
            _library = library;
            _camera = camera;
            _time = time;
            _player = player;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenBlocking(ActionID.TICK, _tick);
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
        }
        
        private UniTask _warmup(ConcurrentContext arg)
        {
            _view = BScene.InstantiateMono(this, _library.playerView, float3.zero, quaternion.identity);

            _view.target.gameObject.SetActive(false);
            
            _player.ChangeTarget += () =>
            {
                _refreshTargetView();
                
                if (_player.Target.y > _player.Position.y)
                {
                    _view.target.SetAsFirstSibling();
                }
                else
                {
                    _view.target.SetAsLastSibling();
                }
            };
            
            
            return UniTask.CompletedTask;
        }

        private void _tick(BlockingContext blockingContext)
        {
            if (_player.IsReady)
            {
                _view.body.position = _camera.WorldToScreen(_player.Position).XY0();
                _view.target.position = _camera.WorldToScreen(_player.Target).XY0();
                
                if (_player.IsMovement)
                {
                    _refreshTargetView();
                }
            }
        }

        private void _refreshTargetView()
        {
            bool isNear = _player.Position.Equals(_player.Target);
         
            if (_view.target.gameObject.activeSelf == isNear)
            {
                _view.target.gameObject.SetActive(!_view.target.gameObject.activeSelf);
            }
            
            string travelTime = _time.CalculateTravelTime(_player.Position, _player.Target, _player.Speed);
            
            float distance = BMath.Distance(_player.Position, _player.Target) * 1000;
            UnityEngine.Debug.Log(distance);   
   
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

            _view.targetText.SetText($"{distanceText}\n{travelTime}");
        }
    }
}