using System;
using Beast.Radar.UI;
using Unity.Mathematics;

namespace Beast.Radar.InputControl
{
    public interface IUIInputController
    {
        public bool IsHardwareInput { get; }
        public event Action ChangedEnable;
        public event Action PauseButtonPressed;
        public event Action SelectButtonPressed;
        public event Action<ESelectionZone> ActionButtonPressed;
        public event Action<int2> DpadPressed;
        public event Action<float2> RightStickMoved;
        public event Action<int> ShoulderPressed;
        public event Action<int> TriggerPressed;
        EInputScheme GetScheme();
    }
}