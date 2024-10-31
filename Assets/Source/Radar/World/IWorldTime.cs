using System;
using Unity.Mathematics;

namespace Beast.Radar.World
{
    public interface IWorldTime
    {
        event Action Changed;
        float GetHours();
        float GetTotalHours();
        float GetMinutes();
        string CalculateTravelTime(float2 a, float2 b, float speed);
    }
}