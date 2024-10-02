using System;
using Beast.Claw.GameCycle;
using Unity.Mathematics;
using IDisposable = Beast.Claw.GameCycle.IDisposable;

namespace Beast.Radar.InputControl
{
    public interface IInputController : Initializable, IDisposable, IFrameBeginUpdate
    {
        public event Action ChangedEarlyValues;
        public float2 ScreenPoint { get; }
        public float2 LastScreenPoint { get; }
        public float2 MoveDelta{ get; }
        public int CurrentTouchCount { get; }
        public int LastTouchCount { get; }
        public bool IsPressMove { get; }
        public float ScrollDelta { get; }
        void Block(bool over);
    }
}