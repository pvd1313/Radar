using System;
using Beast.Claw.GameCycle;
using Beast.Claw.Mathematics;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Beast.Radar.InputControl
{
    public class InputController : IInputController
    {
        public event Action ChangedEarlyValues;
        public float2 LastScreenPoint { get; private set; }
        public float2 ScreenPoint { get; private set; }
        public float2 MoveDelta{ get; private set; }
        public int CurrentTouchCount { get; private set; }
        public int LastTouchCount { get; private set; }
        public bool IsPressMove { get; private set; }
        public float ScrollDelta { get; private set; }

        private readonly IPlayerInputProvider _playerInput;
        private InputActions _actions;
        private float _lastZoomDistance;
        private bool _isBlock;

        public InputController(IPlayerInputProvider inputProvider)
        {
            _playerInput = inputProvider;
        }

        public  UniTask<InitializationResult> Initialize()
        {
            _actions = new InputActions();
            _actions.Enable();
            return InitializationResult.SuccessNotAsync();
        }

        public void Dispose()
        {
            _actions.Dispose();
        }

        public void OnFrameBegin()
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
            if (_playerInput.Scheme is EInputScheme.MouseAndKeyboard)
            {
                ScrollDelta = _actions.Player.Scroll.ReadValue<float>();
                ScreenPoint =_actions.Player.FirstPosition.ReadValue<Vector2>();
                MoveDelta = _actions.Player.Move.ReadValue<Vector2>();
                CurrentTouchCount = !MoveDelta.Equals(float2.zero) || Mouse.current.press.isPressed  ? 1 : 0;
            }
            else if(_playerInput.Scheme is EInputScheme.Gamepad)
            {
                ScrollDelta = _actions.Player.Scroll.ReadValue<float>();
                MoveDelta = _actions.Player.Move.ReadValue<Vector2>();
                ScreenPoint = float2.zero;
                CurrentTouchCount = MoveDelta.Equals(float2.zero) ? 0 : 1;
            }
            else if (_playerInput.Scheme == EInputScheme.Touch)
            {
                CurrentTouchCount = Touchscreen.current.primaryTouch.press.isPressed ? 1 : 0;
                CurrentTouchCount += Touchscreen.current.touches[1].press.isPressed ? 1 : 0;
                switch (CurrentTouchCount)
                {
                    case 0:
                        ScrollDelta = 0;
                        break;
                    case 1:
                        ScreenPoint = _actions.Player.FirstPosition.ReadValue<Vector2>();
                        ScrollDelta = 0;
                        break;
                    case 2:
                        float2 t0 = _actions.Player.FirstPosition.ReadValue<Vector2>();
                        float2 t1 = _actions.Player.SecondPosition.ReadValue<Vector2>();
                        ScreenPoint = (t0 + t1) / 2f;
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
            }
            
            IsPressMove = CurrentTouchCount == 1;
            ChangedEarlyValues?.Invoke();
        }

        private void LateInput()
        {
            if (_playerInput.Scheme is EInputScheme.MouseAndKeyboard)
            {
                _lastZoomDistance = 0f;
                LastScreenPoint = _actions.Player.FirstPosition.ReadValue<Vector2>();
                LastTouchCount = Mouse.current.press.isPressed || _actions.Player.Move.IsPressed()  ? 1 : 0;
            }
            else if(_playerInput.Scheme is  EInputScheme.Gamepad)
            {
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
                        _lastZoomDistance = BMath.Distance(t0, t1);
                        LastScreenPoint = (t0 + t1) / 2f;
                        break;
                }
            }
        }

        public void Block(bool over)
        {
            _isBlock = over;
            if (_isBlock)
            {
                
            }
            else
            {
                LastTouchCount = 0;
                IsPressMove = false;
            }
        }
    }
}
