using Beast.Claw.GameLoop;
using Beast.Radar.Player;
using Beast.Radar.UI;

namespace Beast.Radar.GameTime
{
    public class GameTimeScale : Actor
    {
        private readonly IPlayer _player;
        private readonly IGameTime _time;
        private readonly IWindowManager _windowManager;
        private readonly GameTimeConfig _config;

        public GameTimeScale(
            IPlayer player, 
            IGameTime time, 
            IWindowManager windowManager,
            GameTimeConfig config)
        {
            _player = player;
            _time = time;
            _windowManager = windowManager;
            _config = config;
        }

        protected override void _tick(BlockingContext context)
        {
            if (_windowManager.WindowID != WindowID.HUD)
            {
                _time.SetScale(0);
                return;
            }

            if (_player.IsMoving)
            {
                _time.SetScale(_config.playerWalkingTimeScaleMinutesPerSecond);
            }
            else
            {
                _time.SetScale(_config.playerIdleTimeScaleMinutesPerSecond);
            }
        }
    }
}