using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Radar.GameTime;
using Beast.Radar.Player;
using Beast.Radar.UI;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.World
{
    public class WorldTimeStateController : Controller, IGameCycleListener
    {
        private readonly IPlayerController _player;
        private readonly IGameTime _time;
        private readonly IWindowManager _windowManager;

        public WorldTimeStateController(IPlayerController player, IGameTime time, IWindowManager windowManager)
        {
            _player = player;
            _time = time;
            _windowManager = windowManager;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);   
        }

        private UniTask _warmup(ConcurrentContext arg)
        {
            _windowManager.WindowChanged += windowID =>
            {
                if (windowID == WindowID.MainHUD)
                {
                    _time.Start();
                }
                else
                {
                    _time.Stop();
                }   
            };
            
            _player.ChangeMovementState += () =>
            {
                _time.SetPlayerStateMultiplier(_player.IsMovement);
            };
            
            return UniTask.CompletedTask;
        }
    }
}