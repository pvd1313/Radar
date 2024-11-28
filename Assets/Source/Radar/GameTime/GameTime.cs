using System;
using Beast.Claw.Attributes;
using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Claw.Math;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.GameTime
{
    public class GameTime : Actor, IGameTime
    {
        public DateTime Now => _config.initialDate.Date + TimeSpan.FromMinutes(_timeMinutes);
        public TimeSpan Delta => _delta;
        
        [Required] private readonly GameTimeConfig _config;
        [Required] private readonly GameModel _game;
        [Required] private readonly IFrameTime _frameTime;

        private double _timeMinutes;
        private float _timeMinutesMultiplier;
        private TimeSpan _delta;

        public void SetScale(float minutesPerSecond)
        {
            _timeMinutesMultiplier = minutesPerSecond;
        }

        protected override UniTask _gameLoad(ConcurrentContext context)
        {
            _timeMinutes = _game.timeMinutes;
            _timeMinutesMultiplier = 0;
            
            return UniTask.CompletedTask;
        }

        protected override void _gameSave(BlockingContext context)
        {
            _game.timeMinutes = (ulong)BMath.Floor(_timeMinutes);
        }

        protected override void _tick(BlockingContext context)
        {
            double timeMinutesPrev = _timeMinutes;
            
            _timeMinutes += _timeMinutesMultiplier * _frameTime.Delta;

            _delta = TimeSpan.FromMinutes(_timeMinutes - timeMinutesPrev);
        }
    }
}