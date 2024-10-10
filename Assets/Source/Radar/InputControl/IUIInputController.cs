using System;
using Unity.Mathematics;

namespace Beast.Radar.InputControl
{
    public interface IUIInputController
    {
        public event Action PausePressed;
        public event Action<float2> NavigationPressed;
        public event Action<float2> RightStickPressed;
        public event Action<int> TriggerPressed;
        
    }
}