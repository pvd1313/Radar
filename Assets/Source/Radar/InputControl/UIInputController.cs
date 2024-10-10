using System;
using Beast.Claw.GameCycle;
using Beast.Claw.Mathematics;
using Beast.Radar.GameCycle;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Beast.Radar.InputControl
{
    public class UIInputController : IUIInputController, IGameCycleListener
    {
        public event Action WestPressed;
        public event Action PausePressed;
        public event Action NorthPressed;
        public event Action EastPressed;
        public event Action<float2> NavigationPressed;
        public event Action<float2> RightStickPressed;
        public event Action<int> TriggerPressed;

        private readonly IPlayerInputProvider _input;

        private float2 _lastRightStickValue;

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
            _input.Actions.UI.Cancel.started += OnPressPause;
            _input.Actions.UI.NorthButton.started += OnPressNorth;
            _input.Actions.UI.EastButton.started += OnPressEast;
            _input.Actions.UI.WestButton.started += OnPressWest;
            _input.Actions.UI.Navigate.performed += OnPerformedNavigate;
            _input.Actions.UI.Trigger.started += OnPerformedTrigger;
            _input.Actions.UI.RightStick.performed += OnRightStickPerformed;
        }

        private void UnsubscribeInputActions()
        {
            _input.Actions.UI.Cancel.started -= OnPressPause;
            _input.Actions.UI.NorthButton.started -= OnPressNorth;
            _input.Actions.UI.EastButton.started -= OnPressEast;
            _input.Actions.UI.WestButton.started -= OnPressWest;
            _input.Actions.UI.Navigate.performed -= OnPerformedNavigate;
            _input.Actions.UI.Trigger.started -= OnPerformedTrigger;
            _input.Actions.UI.RightStick.started -= OnRightStickPerformed;
        }

        private void OnPressWest(InputAction.CallbackContext obj)
        {
            WestPressed?.Invoke();
        }

        private void OnPressEast(InputAction.CallbackContext obj)
        {
            EastPressed?.Invoke();
        }

        private void OnPressNorth(InputAction.CallbackContext obj)
        {
            NorthPressed?.Invoke();
        }

        private void OnPressPause(InputAction.CallbackContext obj)
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

        private void OnRightStickPerformed(InputAction.CallbackContext obj)
        {
            float2 value = obj.ReadValue<Vector2>();
            float2 roundValue = RoundValue(value);
            
            if (roundValue.Equals(_lastRightStickValue))
            {
                return;
            }

            if (!roundValue.Equals(float2.zero))
            {
                RightStickPressed?.Invoke(roundValue);
            }
            
            _lastRightStickValue = roundValue;
        }

        private void OnPerformedTrigger(InputAction.CallbackContext obj)
        {
            float value = obj.ReadValue<float>();
            TriggerPressed?.Invoke((int)RoundValue(value));
        }

        private float2 RoundValue(float2 vector)
        {
            return new float2(RoundValue(vector.x), RoundValue(vector.y));
        }

        private float RoundValue(float value)
        {
            return value switch
            {
                > 0.8f => BMath.Ceil(value),
                < -0.8f => BMath.Floor(value),
                _ => 0
            };
        }
    }
}