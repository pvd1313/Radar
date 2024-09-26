using System;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Beast.Radar.Cinematic
{
    public class CameraInput: Initializable
    {
        public Vector3 LastScreenPoint { get; private set; }
        public Vector3 ScreenPointDelta { get; private set; }
        public Vector3 ScreenPoint { get; private set; }
        public int LastTouchCount { get; private set; }
        public int CurrentTouchCount { get; private set; }
        public float Zoom { get; private set; }
        public float LastZoomDistance { get; private set; }
        public bool IsPressMove { get; private set; }
        
        private InputActions _actions;

        private InputScheme _inputScheme;
        

        public UniTask<InitializationResult> Initialize()
        {
            _actions = new InputActions();
            _actions.Enable();
            PlayerInput playerInput = GameObject.FindObjectOfType<PlayerInput>();
            Enum.TryParse(playerInput.currentControlScheme, out _inputScheme);
            Debug.Log(_inputScheme);
            return InitializationResult.SuccessNotAsync();
        }

        public void EarlyInput()
        {
            if (_inputScheme == InputScheme.Mouse)
            {
                ScreenPoint = _actions.Player.FirstPosition.ReadValue<Vector2>();
                Zoom = _actions.Player.Scroll.ReadValue<float>();
                CurrentTouchCount = Mouse.current.press.isPressed ? 1 : 0;
            }
            else
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
                        Vector3 t0 = _actions.Player.FirstPosition.ReadValue<Vector2>();
                        Vector3 t1 = _actions.Player.SecondPosition.ReadValue<Vector2>();
                        ScreenPoint = (t0 + t1) / 2f;
                        float zoomDistance = Vector3.Distance(t0, t1);

                        if (LastZoomDistance == 0f)
                        {
                            Zoom = 0f;
                        }
                        else
                        {
                            Zoom = zoomDistance - LastZoomDistance;
                        }
                        break;
                }
            }

            ScreenPointDelta = ScreenPoint - LastScreenPoint;
            IsPressMove = CurrentTouchCount == 1;
        }

        public void LateInput()
        {
            if (_inputScheme == InputScheme.Mouse)
            {
                LastZoomDistance = 0f;
                LastScreenPoint = _actions.Player.FirstPosition.ReadValue<Vector2>();
                LastTouchCount = Mouse.current.press.isPressed ? 1 : 0;
            }
            else
            {
                LastTouchCount = Touchscreen.current.primaryTouch.press.isPressed ? 1 : 0;
                LastTouchCount += Touchscreen.current.touches[1].press.isPressed ? 1 : 0;
                switch (LastTouchCount)
                {
                    case 0:
                        LastZoomDistance = 0f;
                        break;
                    case 1:
                        LastZoomDistance = 0f;
                        LastScreenPoint = _actions.Player.FirstPosition.ReadValue<Vector2>();
                        break;
                    case 2:
                        Vector3 t0 = _actions.Player.FirstPosition.ReadValue<Vector2>();
                        Vector3 t1 = _actions.Player.SecondPosition.ReadValue<Vector2>();
                        LastScreenPoint = (t0 + t1) / 2f;
                        LastZoomDistance = Vector3.Distance(t0, t1);
                        break;
                }
            }
        }
        
        public void BlockInput(bool isBlock)
        {
            if (isBlock)
            {
                ScreenPointDelta = Vector3.zero;
                LastScreenPoint = Vector3.zero;
                CurrentTouchCount = 0;
                LastTouchCount = 0;
            }
            else
            {
                ScreenPointDelta = ScreenPoint - LastScreenPoint;
                LastScreenPoint = Mouse.current.position.value;
            }
        }
    }
}
