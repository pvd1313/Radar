using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Radar.Player;
using Beast.Radar.UI;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.GameTime
{
    public class GameTimeStateController : Controller, IGameCycleListener
    {
        private readonly IPlayerController _player;
        private readonly IGameTime _time;
        private readonly IWindowManager _windowManager;

        public GameTimeStateController(IPlayerController player, IGameTime time, IWindowManager windowManager)
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
                if (windowID == WindowID.HUD)
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