using System;
using Unity.Mathematics;

namespace Beast.Radar.InputControl
{
    public interface IGameActionMap
    {
        public event Action ChangedEarlyValues;
        InputValueControl<int> TouchCount { get; }
        InputValueControl<float2> FirstTouch { get; }
        InputValueControl<float2> SecondTouch { get; }
        InputValueControl<float2> MoveDelta { get; }
    }
}