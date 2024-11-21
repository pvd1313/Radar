using System;

namespace Beast.Radar.GameTime
{
    public interface IGameTime
    {
        DateTime Now { get; }
        TimeSpan Delta { get; }
        
        void SetScale(float minutesPerSecond);
    }
}