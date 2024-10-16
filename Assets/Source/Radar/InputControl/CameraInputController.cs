using System;
using Beast.Claw.GameCycle;
using Beast.Claw.Math;
using Beast.Radar.GameCycle;
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
        public float2 MoveDelta { get; private set; }
        public int CurrentTouchCount { get; private set; }
        public int LastTouchCount { get; private set; }
        public bool IsPressMove { get; private set; }
        public float ScrollDelta { get; private set; }

        private readonly IPlayerInputProvider _input;
       
        private float _lastZoomDistance;
        private bool _isBlock;

        public CameraInputController(IPlayerInputProvider inputProvider)
        {
            _input = inputProvider;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenUpdate(this, UpdateID.Tick, _tick);
        }

        private void _tick()
        {
            if (_isBlock)
            {
                return;
            }
            
            EarlyInput();   
            LateInput();
        }

        private void EarlyInput()
        {
            switch (_input.Scheme)
            {
                case EInputScheme.MouseAndKeyboard:
                    ScrollDelta = _input.Actions.Player.Scroll.ReadValue<float>();
                    ScreenPoint =_input.Actions.Player.FirstPosition.ReadValue<Vector2>();
                    MoveDelta = _input.Actions.Player.Move.ReadValue<Vector2>();
                    CurrentTouchCount = !MoveDelta.Equals(float2.zero) || Mouse.current.press.isPressed  ? 1 : 0;
                    break;
                
                case EInputScheme.Gamepad:
                    ScrollDelta = _input.Actions.Player.Scroll.ReadValue<float>();
                    MoveDelta = _input.Actions.Player.Move.ReadValue<Vector2>();
                    ScreenPoint = float2.zero;
                    CurrentTouchCount = MoveDelta.Equals(float2.zero) ? 0 : 1;
                    break;
                
                case EInputScheme.Touch:
                    CurrentTouchCount = Touchscreen.current.primaryTouch.press.isPressed ? 1 : 0;
                    CurrentTouchCount += Touchscreen.current.touches[1].press.isPressed ? 1 : 0;
                    switch (CurrentTouchCount)
                    {
                        case 0:
                            ScrollDelta = 0;
                            break;
                    
                        case 1:
                            ScreenPoint = _input.Actions.Player.FirstPosition.ReadValue<Vector2>();
                            ScrollDelta = 0;
                            break;
                    
                        case 2:
                            float2 t0 = _input.Actions.Player.FirstPosition.ReadValue<Vector2>();
                            float2 t1 = _input.Actions.Player.SecondPosition.ReadValue<Vector2>();
                            ScreenPoint = (t0 + t1) * 0.5f;
                            float zoomDistance = BMath.Distance(t0, t1);

                            if (_lastZoomDistance == 0f)
                            {
                                ScrollDelta = 0f;
                            }
                            else
                            {
                                ScrollDelta = zoomDistance - _lastZoomDistance;
                            }
                            break;
                    }

                    break;
            }
            
            IsPressMove = CurrentTouchCount == 1;
            
            ChangedEarlyValues?.Invoke();
        }

        private void LateInput()
        {
            switch (_input.Scheme)
            {
                case EInputScheme.MouseAndKeyboard:
                    _lastZoomDistance = 0f;
                    LastScreenPoint = _input.Actions.Player.FirstPosition.ReadValue<Vector2>();
                    LastTouchCount = Mouse.current.press.isPressed || _input.Actions.Player.Move.IsPressed()  ? 1 : 0;
                    break;
                
                case EInputScheme.Gamepad:
                    _lastZoomDistance = 0f;
                    LastTouchCount = MoveDelta.Equals(float2.zero) ? 0 : 1;
                    break;
                
                case EInputScheme.Touch:
                    LastTouchCount = Touchscreen.current.primaryTouch.press.isPressed ? 1 : 0;
                    LastTouchCount += Touchscreen.current.touches[1].press.isPressed ? 1 : 0;
                    switch (LastTouchCount)
                    {
                        case 0:
                            _lastZoomDistance = 0f;
                            break;
                        
                        case 1:
                            _lastZoomDistance = 0f;
                            LastScreenPoint = _input.Actions.Player.FirstPosition.ReadValue<Vector2>();
                            break;
                        
                        case 2:
                            float2 t0 = _input.Actions.Player.FirstPosition.ReadValue<Vector2>();
                            float2 t1 = _input.Actions.Player.SecondPosition.ReadValue<Vector2>();
                            _lastZoomDistance = BMath.Distance(t0, t1);
                            LastScreenPoint = (t0 + t1) * 0.5f;
                            break;
                    }

                    break;
            }
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
    }
}
