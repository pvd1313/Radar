using System;
using UnityEngine;

namespace Beast.Radar.InputHandling
{
    public class InputControl
    {
        public event Action PressStarted;
        public event Action<float> PressCanceled;
        public EButtonState State { get; private set; }
        public float PressedTime  { get; private set; }
        
        private float _startPressTime;
        
        public void StartPress()
        {
            State = EButtonState.Pressed;
            _startPressTime = Time.time;
            PressStarted?.Invoke();
        }

        public void EndPress()
        {
            State = EButtonState.Default;
            PressedTime = Time.time - _startPressTime;
            PressCanceled?.Invoke(PressedTime);
        }
    }
}