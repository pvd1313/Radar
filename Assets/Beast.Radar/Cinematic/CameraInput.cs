using System;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Mathematics;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Beast.Radar.Cinematic
{
    public class CameraInput: Initializable, Disposable
    {
        public float2 LastScreenPoint { get; private set; }
        public float2 ScreenPointDelta { get; private set; }
        public float2 ScreenPoint { get; private set; }
        public int LastTouchCount { get; private set; }
        public int CurrentTouchCount { get; private set; }
        public bool IsPressMove { get; private set; }
        public float Zoom { get; private set; }
        private float LastZoomDistance { get; set; }
        
        private InputActions _actions;

        private PlayerInputView _playerInputView;

        public CameraInput(PlayerInputView playerInputViewPrefab)
        {
            _playerInputView = playerInputViewPrefab;
        }

        public UniTask<InitializationResult> Initialize()
        {
            _actions = new InputActions();
            _actions.Enable();
            
            PlayerInputView newInstance = BeastScene.NewInstance(_playerInputView, float3.zero, quaternion.identity);
            newInstance.Initialize();
            _playerInputView = newInstance;
            
            return InitializationResult.SuccessNotAsync();
        }

        public UniTask<DispositionResult> Dispose()
        {
            _playerInputView.Dispose();
            _actions.Dispose();

            return DispositionResult.SuccessNotAsync();
        }

        public void EarlyInput()
        {
            if (_playerInputView.Scheme == EInputScheme.Mouse)
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
                        float2 t0 = _actions.Player.FirstPosition.ReadValue<Vector2>();
                        float2 t1 = _actions.Player.SecondPosition.ReadValue<Vector2>();
                        ScreenPoint = (t0 + t1) / 2f;
                        float zoomDistance = BMath.Distance(t0, t1);

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
            if (_playerInputView.Scheme == EInputScheme.Mouse)
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
                        float2 t0 = _actions.Player.FirstPosition.ReadValue<Vector2>();
                        float2 t1 = _actions.Player.SecondPosition.ReadValue<Vector2>();
                        LastScreenPoint = (t0 + t1) / 2f;
                        LastZoomDistance = BMath.Distance(t0, t1);
                        break;
                }
            }
        }

        public void BlockInput(bool isBlock)
        {
            if (isBlock)
            {
                ScreenPointDelta = float2.zero;
                LastScreenPoint = float2.zero;
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
