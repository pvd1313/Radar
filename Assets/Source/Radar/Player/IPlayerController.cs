using System;
using Beast.Claw.Common;
using Unity.Mathematics;

namespace Beast.Radar.Player
{
    public interface IPlayerController : IReady
    {
        event Action ChangeMovementState;
        event Action ChangeTarget;
        float2 TempPosition { get; }
        float2 Position { get; }
        float2 Target { get; }
        float Speed { get; }
        bool IsMovement { get; }
        void SwitchMovement();
    }
}