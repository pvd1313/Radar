using Unity.Mathematics;

namespace Beast.Radar.Player
{
    public interface IPlayerMovement
    {
        float2 Position { get; }
        float2 Target { get; }
        float Speed { get; }
    }
}