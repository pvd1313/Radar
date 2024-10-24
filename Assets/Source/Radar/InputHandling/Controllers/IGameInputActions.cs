using System;
using Unity.Mathematics;

namespace Beast.Radar.InputHandling
{
    public interface IGameInputActions
    {
        public event Action ChangedEarlyValues;
        
        public InputValueControl<float2> ScreenPoint { get; }
        public InputValueControl<float2> LastScreenPoint { get; }
        public InputValueControl<float2> MoveDelta{ get; }
        public InputValueControl<int> CurrentTouchCount { get; }
        public InputValueControl<int> LastTouchCount { get; }
        public InputValueControl<float> Scroll { get; }
        public bool IsPressMove { get; }
        
        void ChangeBlock(bool isBlock);
    }
}