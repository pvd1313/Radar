using System;
using Beast.Claw.GameCycle;
using Beast.Claw.Mathematics;
using Beast.Radar.Cinematic;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Beast.Radar.InputControl
{
    public class InputController : IInputController
    {
        public event Action ChangedEarlyValues;
        
        public float2 ScreenPoint { get; private set; }
        public float2 LastScreenPoint { get; private set; }
        public float2 MoveDelta{ get; private set; }
        public int CurrentTouchCount { get; private set; }
        public int LastTouchCount { get; private set; }
        public bool IsPressMove { get; private set; }
        public float Zoom { get; private set; }

        private readonly IPlayerInputProvider _playerInput;
        private InputActions _actions;

        private float2 _screenCenter;
        private float _lastZoomDistance;
        
        public InputController(IPlayerInputProvider inputProvider)
        {
            _playerInput = inputProvider;
        }

        public  UniTask<InitializationResult> Initialize()
        {
            _actions = new InputActions();
            _actions.Enable();
            _screenCenter = new float2((float)Screen.width / 2,(float)Screen.height/ 2);
            return InitializationResult.SuccessNotAsync();
        }

        public UniTask<DispositionResult> Dispose()
        {
            _actions.Dispose();
            return DispositionResult.SuccessNotAsync();
        }

        public void OnFrameBegin()
        {
            EarlyInput();   
        }

        public void OnFrameEnd()
        {
            LateInput();
        }

        private void EarlyInput()
        {
            if (_playerInput.Scheme == EInputScheme.Mouse)
            {
                ScreenPoint = _actions.Player.FirstPosition.ReadValue<Vector2>();
                Zoom = _actions.Player.Scroll.ReadValue<float>();
                CurrentTouchCount = Mouse.current.press.isPressed  ? 1 : 0;
            }
            else if(_playerInput.Scheme is EInputScheme.Keyboard or EInputScheme.Gamepad)
            {
                ScreenPoint = _screenCenter;
                MoveDelta = _actions.Player.Move.ReadValue<Vector2>() * -1;
                Zoom = _actions.Player.Scroll.ReadValue<float>();
                CurrentTouchCount = MoveDelta.Equals(float2.zero) ? 0 : 1;
            }
            else if (_playerInput.Scheme == EInputScheme.Touch)
            {
                CurrentTouchCount = Touchscreen.current.primaryTouch.press.isPressed ? 1 : 0;
                CurrentTouchCount += Touchscreen.current.touches[1].press.isPressed ? 1 : 0;
                switch (CurrentTouchCount)
                {
                    case 1:
                        ScreenPoint = _actions.Player.FirstPosition.ReadValue<Vector2>();
                        Zoom = 0;
                        break;
                    case 2:
                        float2 t0 = _actions.Player.FirstPosition.ReadValue<Vector2>();
                        float2 t1 = _actions.Player.SecondPosition.ReadValue<Vector2>();
                        ScreenPoint = (t0 + t1) / 2f;
                        float zoomDistance = BMath.Distance(t0, t1);

                        if (_lastZoomDistance == 0f)
                        {
                            Zoom = 0f;
                        }
                        else
                        {
                            Zoom = zoomDistance - _lastZoomDistance;
                        }
                        break;
                }
            }
            
            _screenCenter = MoveDelta.Equals(float2.zero)
                ? ScreenPoint - LastScreenPoint 
                : MoveDelta;
            IsPressMove = CurrentTouchCount == 1;
            ChangedEarlyValues?.Invoke();
        }

        private void LateInput()
        {
            if (_playerInput.Scheme == EInputScheme.Mouse)
            {
                _lastZoomDistance = 0f;
                LastScreenPoint = _actions.Player.FirstPosition.ReadValue<Vector2>();
                LastTouchCount = Mouse.current.press.isPressed ? 1 : 0;
            }
            else if(_playerInput.Scheme is EInputScheme.Keyboard or EInputScheme.Gamepad)
            {
                LastScreenPoint = _screenCenter;
                _lastZoomDistance = 0f;
                LastTouchCount = MoveDelta.Equals(float2.zero) ? 0 : 1;
            }
            else if(_playerInput.Scheme == EInputScheme.Touch)
            {
                LastTouchCount = Touchscreen.current.primaryTouch.press.isPressed ? 1 : 0;
                LastTouchCount += Touchscreen.current.touches[1].press.isPressed ? 1 : 0;
                switch (LastTouchCount)
                {
                    case 0:
                        _lastZoomDistance = 0f;
                        break;
                    case 1:
                        _lastZoomDistance = 0f;
                        LastScreenPoint = _actions.Player.FirstPosition.ReadValue<Vector2>();
                        break;
                    case 2:
                        float2 t0 = _actions.Player.FirstPosition.ReadValue<Vector2>();
                        float2 t1 = _actions.Player.SecondPosition.ReadValue<Vector2>();
                        LastScreenPoint = (t0 + t1) / 2f;
                        _lastZoomDistance = BMath.Distance(t0, t1);
                        break;
                }
            }
        }
    }
}
