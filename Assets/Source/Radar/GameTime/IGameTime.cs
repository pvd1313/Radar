using Unity.Mathematics;

namespace Beast.Radar.GameTime
{
    public interface IGameTime
    {
        float GetHours();
        float GetTotalHours();
        float GetMinutes();
        string CalculateTravelTime(float2 a, float2 b, float speed);
    }
}