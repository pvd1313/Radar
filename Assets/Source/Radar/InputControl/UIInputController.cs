using System;
using Beast.Claw.GameCycle;
using Beast.Radar.GameCycle;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Beast.Radar.InputControl
{
    public class UIInputController : IUIInputController, IGameCycleListener
    {
        public event Action<float2> NavigationPressed;
        public event Action<float2> RightStickPressed;
        public event Action<int> TriggerPressed;
        public event Action PausePressed;

        private readonly IPlayerInputProvider _input;

        public UIInputController(IPlayerInputProvider inputProvider)
        {
            _input = inputProvider;
        }

        public void ListenGameCycle(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Warmup, Warmup);
            handle.ListenOperation(this, OperationID.Dispose, Dispose);
        }

        private UniTask<OperationResult> Warmup()
        {
            _input.Actions.UI.Enable();
            SubscribeInputActions();
            return OperationResult.SuccessSync();
        }

        private UniTask<OperationResult> Dispose()
        {
            _input.Actions.UI.Disable();

            UnsubscribeInputActions();
            return OperationResult.SuccessSync();
        }

        private void SubscribeInputActions()
        {
            _input.Actions.UI.Cancel.started += OnStartedPause;
            _input.Actions.UI.Navigate.performed += OnPerformedNavigate;
            _input.Actions.UI.Trigger.started += OnPerformedTrigger;
        }

        private void UnsubscribeInputActions()
        {
            _input.Actions.UI.Cancel.started -= OnStartedPause;
            _input.Actions.UI.Navigate.performed += OnPerformedNavigate;
        }

        private void OnStartedPause(InputAction.CallbackContext obj)
        {
            PausePressed?.Invoke();
        }

        private void OnPerformedNavigate(InputAction.CallbackContext obj)
        {
            float2 value = obj.ReadValue<Vector2>();
            if (value.Equals(float2.zero))
            {
                return;
            }
            NavigationPressed?.Invoke(value);
        }

        private void OnPerformedTrigger(InputAction.CallbackContext obj)
        {
            float value = obj.ReadValue<float>();
            int roundValue = value > 0 ? 1 : -1;
            TriggerPressed?.Invoke(roundValue);
        }
    }
}