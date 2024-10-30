using Beast.Claw.Common;
using Unity.Mathematics;

namespace Beast.Radar.Player
{
    public interface IPlayerMovement : IReady
    {
        float2 Position { get; }
        float2 Target { get; }
        float Speed { get; }
    }
}