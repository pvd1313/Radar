using System;
using UnityEngine;

namespace Beast.Radar.InputControl
{
    public class InputValueControl<TValue>
    {
        public event Action<TValue> PressStarted;
        public event Action<TValue> PressPerformed;
        public event Action<TValue,float> PressCanceled;
        
        public EButtonState State { get; private set; }
        public TValue Value { get; private set; }
        
        private float _startPressTime;

        public void StartPress(TValue value)
        {
            State = EButtonState.Pressed;
            Value = value;
            _startPressTime = Time.time;
            PressStarted?.Invoke(Value);
        }

        public void PerformPress(TValue value)
        {
            Value = value;
            PressPerformed?.Invoke(Value);
        }

        public void EndPress(TValue value)
        {
            State = EButtonState.Default;
            Value = value;
            PressCanceled?.Invoke(Value, Time.time - _startPressTime);
        }

        public void SetValue(TValue value)
        {
            Value = value;
        }
    }
}