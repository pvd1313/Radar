using System;
using Unity.Mathematics;

namespace Beast.Radar.InputHandling
{
    public interface IUIInputActions
    {
        public event Action ChangedEnable;
        public event Action<int2> NavigatePressed;
        public event Action<float2> RightStickMoved;
        
        InputValueControl<EInputOptionButton> OptionButtons { get; }
        InputValueControl<int> Trigger{ get; }
        InputValueControl<int> Shoulder{ get; }
        
    }
}