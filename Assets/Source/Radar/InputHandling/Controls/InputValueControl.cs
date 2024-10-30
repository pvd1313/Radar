using System;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.InputHandling
{
    public class InputValueControl<TValue>
    {
        private const float MULTI_TAP_COOLDOWN = 0.2f; 
        public event Action<TValue> PressStarted;
        public event Action<TValue> PressPerformed;
        public event Action<TValue,float> PressCanceled;
        
        public EButtonState State { get; private set; }
        public TValue Value { get; private set; }
        public TValue LastValue { get; private set; }
        public float PressedTime { get; private set; }
        public int TapCount { get; private set; }

        private float _endPressedTime;
        private float _startPressTime;

        public void StartPress(TValue value)
        {
            _startPressTime = Time.time;
        
            if (_startPressTime - _endPressedTime <= MULTI_TAP_COOLDOWN)
            {
                TapCount++;
            }
            else
            {
                TapCount = 1;
            }
  
            State = EButtonState.Pressed;
            Value = value;
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
            _endPressedTime = Time.time;
            PressedTime = _endPressedTime - _startPressTime;
            PressCanceled?.Invoke(Value, PressedTime);
        }

        public void SetValue(TValue value)
        {
            Value = value;
        }
    }
}