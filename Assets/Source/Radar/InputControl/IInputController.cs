using System;
using Beast.Claw.GameCycle;
using Unity.Mathematics;

namespace Beast.Radar.InputControl
{
    public interface IInputController : Initializable, Disposable, IFrameBeginUpdate
    {
        public event Action ChangedEarlyValues;
        public float2 ScreenPoint { get; }
        public float2 MoveDelta{ get; }
        public int CurrentTouchCount { get; }
        public int LastTouchCount { get; }
        public bool IsPressMove { get; }
        public float ZoomAxis { get; }
    }
}