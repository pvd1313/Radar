using System;

namespace Beast.Radar.GameTime
{
    public interface IGameTime
    {
        event Action MinuteChanged;
        void SetPlayerStateMultiplier(bool onAction);
        void Stop();
        void Start();
        TimeSpan GetTime();
    }
}