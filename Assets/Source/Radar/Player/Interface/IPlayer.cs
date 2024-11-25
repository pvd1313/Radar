using Beast.Claw.Common;
using Unity.Mathematics;

namespace Beast.Radar.Player
{
    public interface IPlayer : IReady
    {
        float2 Position { get; }
        
        float2 Target { get; }
        
        bool IsMoving { get; }
        
        bool HasTarget { get; }
        
        void ToggleMovementState();

        float GetRemainingDistanceToTarget();
    }
}