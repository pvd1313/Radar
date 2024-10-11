using System;
using Beast.Radar.UI;
using Unity.Mathematics;

namespace Beast.Radar.InputControl
{
    public interface IUIInputController
    {
        public bool IsHardwareInput { get; }
        public event Action IsEnabledChanged;
        public event Action<ESelectionZone> ActionButtonPressed;
        public event Action PauseButtonPressed;
        public event Action<int2> DpadPressed;
        public event Action<int2> RightStickMoved;
        public event Action<int> TriggerPressed;
    }
}