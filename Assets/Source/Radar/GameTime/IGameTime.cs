using System;

namespace Beast.Radar.GameTime
{
    public interface IGameTime
    {
        event Action Changed;
        void SetPlayerStateMultiplier(bool onAction);
        void Stop();
        void Start();
        TimeSpan GetTime();
    }
}