using System;
using Unity.Mathematics;

namespace Beast.Radar.InputControl
{
    public interface ICameraInputController
    {
        public event Action ChangedEarlyValues;
        public InputValueControl<float2> ScreenPoint { get; }
        public InputValueControl<float2> LastScreenPoint { get; }
        public InputValueControl<float2>  MoveDelta{ get; }
        public InputValueControl<int> CurrentTouchCount { get; }
        public InputValueControl<int> LastTouchCount { get; }
        public bool IsPressMove { get; }
        public InputValueControl<float>  ScrollDelta { get; }
        void ChangeBlock(bool isBlock);
    }
}