using System;
using Unity.Mathematics;

namespace Beast.Radar.InputHandling
{
    public interface IUIInputActions
    {
        public event Action ChangedEnable;
        
        InputValueControl<EInputOptionButton> OptionButtons { get; }
        InputValueControl<int> Trigger{ get; }
        InputValueControl<int> Shoulder{ get; }
        InputValueControl<int2> MainNavigation { get; }
        InputValueControl<float2> OptionNavigation { get; }
    }
}