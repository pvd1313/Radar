using System;
using Unity.Mathematics;

namespace Beast.Radar.InputControl
{
    public interface IBeastUIInputController
    {
        public event Action PausePressed;
        public event Action<float2> NavigationPressed;
    }
}