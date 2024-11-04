using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Beast.Radar.GameTime;
using Beast.Radar.Player;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

namespace Beast.Radar.UI
{
    public class PlayerViewController : Controller, IGameCycleListener 
    {
        private readonly CorePrefabLibrary _library;
        private readonly ICameraTransform _camera;
        private readonly IGameTime _time;
        private readonly IPlayerMovement _player;

        private PlayerView _view;

        public PlayerViewController(CorePrefabLibrary library, ICameraTransform camera, IGameTime time, IPlayerMovement player)
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
            return UniTask.CompletedTask;
        }

        private void _tick(BlockingContext blockingContext)
        {
            if (_player.IsReady)
            {
                _view.body.position = _camera.WorldToScreen(_player.Position).XY0();
                _view.target.position = _camera.WorldToScreen(_player.Target).XY0();

                if (_view.target.gameObject.activeSelf != BMath.Distance(_player.Position, _player.Target) > 0.1f)
                {
                    _view.target.gameObject.SetActive(!_view.target.gameObject.activeSelf);
                }
                
                float distance = BMath.Distance(_player.Position, _player.Target);
                string travelTime = _time.CalculateTravelTime(_player.Position, _player.Target, _player.Speed);
                _view.targetText.SetText($"{distance:F0}\n{travelTime}");
                
                //----------------------- from event
                if (_player.Target.y > _player.Position.y)
                {
                    _view.target.SetAsFirstSibling();
                }
                else
                {
                    _view.target.SetAsLastSibling();
                }
            }
        }
    }
}