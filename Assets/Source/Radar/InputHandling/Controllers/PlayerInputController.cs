using System;
using Beast.Claw.GameCycle;
using Beast.Radar.Cinematic;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Beast.Radar.InputControl
{
    public class PlayerInputController : IPlayerInputController, IGameCycleListener
    {
        public event Action<float2> ClickMove;

        private readonly ICameraTransform _cameraTransform;
        private readonly IInputSystem _inputActions;

        private float _startClickTime;
        public PlayerInputController(ICameraTransform cameraTransform, IInputSystem inputActions)
        {
            _cameraTransform = cameraTransform;
            _inputActions = inputActions;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(OperationID.WARMUP, _warmup);
            handle.ListenOperation(OperationID.DISPOSE, _dispose);
        }

        private UniTask<OperationResult> _warmup()
        {
            _inputActions.Actions.Player.Click.started += _onClickStarted;
            _inputActions.Actions.Player.Click.canceled += _onClickCanceled;
            
            return OperationResult.SuccessSync();
        }

        private void _onClickStarted(InputAction.CallbackContext obj)
        {
            _startClickTime = Time.time;
        }

        private void _onClickCanceled(InputAction.CallbackContext obj)
        {
            float endClickTime = Time.time;
            if (endClickTime - _startClickTime <= _inputActions.ClickDuration)
            {
                float2 mousePosition = _inputActions.Actions.Player.FirstPosition.ReadValue<Vector2>();
                ClickMove?.Invoke(mousePosition);
            }
        }

        private UniTask<OperationResult> _dispose()
        {
            return OperationResult.SuccessSync();
        }
    }
}