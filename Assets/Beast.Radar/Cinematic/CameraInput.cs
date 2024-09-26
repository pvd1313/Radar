using System;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Beast.Radar.Cinematic
{
    public class CameraInput: Initializable
    {
        private enum Phase
        {
            Early,
            Late
        }

        private enum Scheme
        {
            Mouse,
            Touch,
        }

        public Vector3 CurrentScreenPoint { get; private set; }
        public Vector3 LastScreenPoint { get; private set; }
        public Vector3 ScreenPointDelta { get; private set; }
        public int LastTouchCount { get; private set; }
        public int CurrentTouchCount { get; private set; }
        public float ZoomDelta { get; private set; }
        public float LastZoomDistance { get; private set; }

        private InputActions _actions;
        private readonly Camera _camera;

        private bool _isMouseButtonDown;
        private Phase _phase;
        private Scheme _scheme;

        public CameraInput(Camera camera)
        {
            _camera = camera;
        }

        public UniTask<InitializationResult> Initialize()
        {
            _actions = new InputActions();
            _actions.Enable();
            PlayerInput playerInput = GameObject.FindObjectOfType<PlayerInput>();
            Enum.TryParse(playerInput.currentControlScheme, out _scheme);
            Debug.Log(_scheme);
            return InitializationResult.SuccessNotAsync();
        }

        public void EarlyInput()
        {
            _phase = Phase.Early;

            if (_scheme == Scheme.Mouse)
            {
                CurrentScreenPoint = _actions.Player.FirstPosition.ReadValue<Vector2>();
                ZoomDelta = _actions.Player.Scroll.ReadValue<float>();
                CurrentTouchCount = Mouse.current.press.isPressed ? 1 : 0;
            }
            else
            {
                CurrentTouchCount = Touchscreen.current.primaryTouch.press.isPressed ? 1 : 0;
                CurrentTouchCount += Touchscreen.current.touches[1].press.isPressed ? 1 : 0;
                switch (CurrentTouchCount)
                {
                    case 1:
                        CurrentScreenPoint = _actions.Player.FirstPosition.ReadValue<Vector2>();
                        ZoomDelta = 0;
                        break;
                    case 2:
                        Vector3 t0 = _actions.Player.FirstPosition.ReadValue<Vector2>();
                        Vector3 t1 = _actions.Player.SecondPosition.ReadValue<Vector2>();
                        CurrentScreenPoint = (t0 + t1) / 2f;
                        float zoomDistance = Vector3.Distance(t0, t1);

                        if (LastZoomDistance == 0f)
                        {
                            ZoomDelta = 0f;
                        }
                        else
                        {
                            ZoomDelta = zoomDistance - LastZoomDistance;
                        }
                        break;
                }
            }

            ScreenPointDelta = CurrentScreenPoint - LastScreenPoint;
            _isMouseButtonDown = CurrentTouchCount > 0;
        }

        public void LateInput()
        {
            _phase = Phase.Late;

            if (_scheme == Scheme.Mouse)
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

        public bool IsMoving()
        {
            return _isMouseButtonDown;
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
                ScreenPointDelta = CurrentScreenPoint - LastScreenPoint;
                LastScreenPoint = Mouse.current.position.value;
            }
        }
    }
}
