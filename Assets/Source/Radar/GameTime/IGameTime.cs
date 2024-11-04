using System;
using Unity.Mathematics;

namespace Beast.Radar.GameTime
{
    public interface IGameTime
    {
        event Action Changed;
        void SetPlayerStateMultiplier(bool onAction);
        void Stop();
        void Start();
        float GetHours();
        float GetTotalHours();
        float GetMinutes();
        string CalculateTravelTime(float2 a, float2 b, float speed);
    }
}