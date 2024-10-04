using System;
using Beast.Claw.GameCycle;
using Unity.Mathematics;

namespace Beast.Radar.InputControl
{
    public interface IBeastUIInputController : IBeastInitialize, IBeastDispose, IBeastFrameBegin
    {
        public event Action PausePressed;
        public event Action<float2> NavigationPressed;
    }
}