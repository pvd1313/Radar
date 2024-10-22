using System;
using Beast.Claw.Debug;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Beast.Radar.InputControl
{
    public class GameActionMap : IGameActionMap
    {
        public event Action ChangedEarlyValues;

        public InputValueControl<int> TouchCount { get; } = new();
        public InputValueControl<float2> FirstTouch { get; } = new();
        public InputValueControl<float2> SecondTouch { get; } = new();
        public InputValueControl<float2> MoveDelta { get; } = new();

        private readonly IInputSystem _inputSystem;
        private readonly InputActions _actions;

        public GameActionMap(IInputSystem inputSystem, InputActions actions)
        {
            _inputSystem = inputSystem;
            _actions = actions;
        }

        public void Warmup()
        {
            _actions.Player.Enable();
        }
        
        public void Update()
        {
            int touchCount = 0;
            switch (_inputSystem.Scheme)
            {
                case EInputScheme.MouseAndKeyboard:
                    touchCount = Mouse.current.press.isPressed  ? 1 : 0;
                    break;
                
                case EInputScheme.Gamepad:
                    break;

                case EInputScheme.Touch:
                    touchCount = Touchscreen.current.primaryTouch.press.isPressed ? 1 : 0;
                    touchCount += Touchscreen.current.touches[1].press.isPressed ? 1 : 0;
                    break;
            }
            
            _updateTouchCount(touchCount);
            _updateFirstTouchPosition(touchCount);
            _updateSecondTouchPosition(touchCount);
            _updateMoveDelta();
            
            BDebug.Track(this, "touch count", TouchCount.Value);
            BDebug.Track(this, "first touch", FirstTouch.Value);
            BDebug.Track(this, "second touch", SecondTouch.Value);
            
        }

        private void _updateTouchCount(int touchCount)
        {
            switch (touchCount)
            {
                case 0 when TouchCount.State == EButtonState.Default:
                    return;
                
                case 0 when TouchCount.State == EButtonState.Pressed:
                    TouchCount.EndPress(touchCount);
                    break;
                
                case 1 or 2 when TouchCount.State == EButtonState.Default:
                    TouchCount.StartPress(touchCount);
                    break;
                
                case 1 or 2 when TouchCount.State == EButtonState.Pressed:
                    TouchCount.PerformPress(touchCount);
                    break;
            }
        }

        private void _updateFirstTouchPosition(int touchCount)
        {
            float2 position = (float2)_actions.Player.FirstPosition.ReadValue<Vector2>();
        
            switch (touchCount)
            {
                case 0 when FirstTouch.State == EButtonState.Default:
                    return;
                
                case 0 when FirstTouch.State == EButtonState.Pressed:
                    FirstTouch.EndPress(position);
                    FirstTouch.SetValue(float2.zero);
                    break;
                
                case 1 when FirstTouch.State == EButtonState.Default:
                    FirstTouch.StartPress(position);
                    break;
                
                case 1 when FirstTouch.State == EButtonState.Pressed:
                    FirstTouch.PerformPress(position);
                    break;
            }
        }

        private void _updateSecondTouchPosition(int touchCount)
        {
            float2 position = (float2)_actions.Player.SecondPosition.ReadValue<Vector2>();
        
            switch (touchCount)
            {
                case 0 or 1 when SecondTouch.State == EButtonState.Default:
                    return;
                
                case 0 or 1 when SecondTouch.State == EButtonState.Pressed:
                    SecondTouch.EndPress(position);
                    SecondTouch.SetValue(float2.zero);
                    break;
                
                case 2 when SecondTouch.State == EButtonState.Default:
                    SecondTouch.StartPress(position);
                    break;
                
                case 2 when SecondTouch.State == EButtonState.Pressed:
                    SecondTouch.PerformPress(position);
                    break;
            }
        }

        private void _updateMoveDelta()
        {
            float2 delta = (float2)_actions.Player.Move.ReadValue<Vector2>();
            bool isPress = !delta.Equals(float2.zero);
;            
            switch (isPress)
            {
                case false when MoveDelta.State == EButtonState.Default:
                    return;
                
                case false when MoveDelta.State == EButtonState.Pressed:
                    MoveDelta.EndPress(delta);
                    MoveDelta.SetValue(float2.zero);
                    break;
              
                case true when MoveDelta.State == EButtonState.Default:
                    MoveDelta.StartPress(delta);
                    break;
                
                case true when MoveDelta.State == EButtonState.Pressed:
                    MoveDelta.PerformPress(delta);
                    break;
            }
            
        }

        public void Dispose()
        {
            
        }
    }
}