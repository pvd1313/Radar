using System;
using UnityEngine;

namespace Beast.Radar.InputHandling
{
    public class InputValueControl<TValue>
    {
        public event Action<TValue> PressStarted;
        public event Action<TValue> PressPerformed;
        public event Action<TValue,float> PressCanceled;
        
        public EButtonState State { get; private set; }
        public TValue Value { get; private set; }
        public TValue LastValue { get; private set; }
        public float PressedTime{ get; private set; }
        
        private float _startPressTime;

        public void StartPress(TValue value)
        {
            State = EButtonState.Pressed;
            Value = value;
            _startPressTime = Time.time;
            PressedTime = Time.time - _startPressTime;
            PressStarted?.Invoke(Value);
        }

        public void PerformPress(TValue value)
        {
            Value = value;
            PressedTime = Time.time - _startPressTime;
            PressPerformed?.Invoke(Value);
        }

        public void EndPress(TValue value)
        {
            State = EButtonState.Default;
            LastValue = Value;
            Value = value;
            PressedTime = Time.time - _startPressTime;
            PressCanceled?.Invoke(Value, PressedTime);
        }

        public void SetValue(TValue value)
        {
            Value = value;
        }
    }
}