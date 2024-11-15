using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Radar.Player;
using Beast.Radar.UI;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.GameTime
{
    public class GameTimeStateController : Actor
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

        protected override UniTask _warmup(ConcurrentContext arg)
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