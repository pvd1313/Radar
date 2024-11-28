using Beast.Claw.Attributes;
using Beast.Claw.GameLoop;
using Beast.Radar.Player;
using Beast.Radar.UI;

namespace Beast.Radar.GameTime
{
    public class GameTimeScale : Actor
    {
        [Required] private readonly IPlayer _player;
        [Required] private readonly IGameTime _time;
        [Required] private readonly IWindowManager _windowManager;
        [Required] private readonly GameTimeConfig _config;

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