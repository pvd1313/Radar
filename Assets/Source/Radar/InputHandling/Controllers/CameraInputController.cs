using System;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Math;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Beast.Radar.InputControl
{
    public class CameraInputController : ICameraInputController, IGameCycleListener
    {
        public event Action ChangedEarlyValues;
        
        public float2 LastScreenPoint { get; private set; }
        public float2 ScreenPoint { get; private set; }
        public InputValueControl<float2> MoveDelta { get; private set; } = new();
        public int CurrentTouchCount { get; private set; }
        public int LastTouchCount { get; private set; }
        public bool IsPressMove { get; private set; }
        public InputValueControl<float> ScrollDelta { get; private set; } = new();

        private readonly IInputSystem _inputSystem;
        

        private float2 _startClickTime;
        private float _lastZoomDistance;
        private bool _isBlock;
        private  InputActions _gameActions;

        public CameraInputController(IInputSystem inputSystem)
        {
            _inputSystem = inputSystem;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenUpdate(UpdateID.TICK, _tick);
        }

        public void ChangeBlock(bool isBlock)
        {
            _isBlock = isBlock;
            
            if (_isBlock)
            {
                LastTouchCount = 0;
                IsPressMove = false;
            }
        }

        private void _tick()
        {
            _gameActions ??= _inputSystem.Actions;
            if (_isBlock)
            {
                return;
            }
            
            _earlyInput();   
            _lateInput();
            BDebug.Track(this, "Touch count", CurrentTouchCount);
            BDebug.Track(this, "Last touch count", LastTouchCount);
            BDebug.Track(this, "Screen point", ScreenPoint);
            BDebug.Track(this, "Last screen point", LastScreenPoint);
        }

        private void _earlyInput()
        {
            switch (_inputSystem.Scheme)
            {
                case EInputScheme.MouseAndKeyboard:
                    ScrollDelta.UpdateValue(_gameActions.Player.Scroll.ReadValue<float>());
                    ScreenPoint = _gameActions.Player.FirstPosition.ReadValue<Vector2>();
                    MoveDelta.UpdateValue((float2)_gameActions.Player.Move.ReadValue<Vector2>());
                    CurrentTouchCount = MoveDelta.State == EButtonState.Pressed ? 1 : Mouse.current.press.isPressed  ? 1 : 0;
                    break;
                
                case EInputScheme.Gamepad:
                    ScrollDelta.UpdateValue(_gameActions.Player.Scroll.ReadValue<float>());
                    MoveDelta.UpdateValue((float2)_gameActions.Player.Move.ReadValue<Vector2>());
                    ScreenPoint = _getScreenCenter();
                    CurrentTouchCount = MoveDelta.State == EButtonState.Pressed ? 1 : 0;
                    break;
                
                case EInputScheme.Touch:
                    CurrentTouchCount = _getTouchCount();
                    switch (CurrentTouchCount)
                    {
                        case 0:
                            ScrollDelta.UpdateValue(0);
                            break;
                    
                        case 1:
                            ScreenPoint = _inputSystem.Actions.Player.FirstPosition.ReadValue<Vector2>();
                            ScrollDelta.UpdateValue(0);
                            break;
                    
                        case 2:
                            float2 t0 = _inputSystem.Actions.Player.FirstPosition.ReadValue<Vector2>();
                            float2 t1 = _inputSystem.Actions.Player.SecondPosition.ReadValue<Vector2>();
                            ScreenPoint = (t0 + t1) * 0.5f;
                            float zoomDistance = BMath.Distance(t0, t1);
                            ScrollDelta.UpdateValue(_lastZoomDistance == 0f ? 0 : zoomDistance - _lastZoomDistance);
                            break;
                    }

                    break;
            }
            
            IsPressMove = CurrentTouchCount == 1;
            
            ChangedEarlyValues?.Invoke();
        }

        private void _lateInput()
        {
            switch (_inputSystem.Scheme)
            {
                case EInputScheme.MouseAndKeyboard:
                    _lastZoomDistance = 0f;
                    LastScreenPoint = _gameActions.Player.FirstPosition.ReadValue<Vector2>();
                    LastTouchCount = Mouse.current.press.isPressed || _gameActions.Player.Move.IsPressed()  ? 1 : 0;
                    break;
                
                case EInputScheme.Gamepad:
                    _lastZoomDistance = 0f;
                    LastScreenPoint = _getScreenCenter();
                    LastTouchCount = MoveDelta.State == EButtonState.Pressed ? 1 : 0;
                    break;
                
                case EInputScheme.Touch:
                    LastTouchCount = _getTouchCount();
                    switch (LastTouchCount)
                    {
                        case 0:
                            _lastZoomDistance = 0f;
                            break;
                        
                        case 1:
                            _lastZoomDistance = 0f;
                            LastScreenPoint = _inputSystem.Actions.Player.FirstPosition.ReadValue<Vector2>();
                            break;
                        
                        case 2:
                            float2 t0 = _inputSystem.Actions.Player.FirstPosition.ReadValue<Vector2>();
                            float2 t1 = _inputSystem.Actions.Player.SecondPosition.ReadValue<Vector2>();
                            _lastZoomDistance = BMath.Distance(t0, t1);
                            LastScreenPoint = (t0 + t1) * 0.5f;
                            break;
                    }

                    break;
            }
        }

        private float2 _getScreenCenter()
        {
            return new float2((float)Screen.width / 2, (float)Screen.height / 2);
        }

        private int _getTouchCount()
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
            return touchCount;
        }
    }
}
