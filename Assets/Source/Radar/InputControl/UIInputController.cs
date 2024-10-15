using System;
using Beast.Claw.GameCycle;
using Beast.Claw.Math;
using Beast.Radar.GameCycle;
using Beast.Radar.UI;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Beast.Radar.InputControl
{
    public class UIInputController : IUIInputController, IGameCycleListener
    {
        public bool IsHardwareInput { get; private set; }
        public event Action ChangedEnable;
        public event Action PauseButtonPressed;
        public event Action SelectButtonPressed;
        public event Action<ESelectionZone> ActionButtonPressed;
        public event Action<int2> DpadPressed;
        public event Action<float2> RightStickMoved;
        public event Action<int> ShoulderPressed;
        public event Action<int> TriggerPressed;

        private readonly IPlayerInputProvider _input;

        private int2 _lastRightStickValue;

        public UIInputController(IPlayerInputProvider inputProvider)
        {
            _input = inputProvider;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Warmup, _warmup);
            handle.ListenOperation(this, OperationID.Dispose, _dispose);
            handle.ListenUpdate(this, UpdateID.Tick, _tick);
        }

        private UniTask<OperationResult> _warmup()
        {
            SubscribeToInputSchemeChange();

            IsHardwareInput = _input.Scheme is not EInputScheme.Touch;

            if (IsHardwareInput)
            {
                SubscribeInputActions();
            }

            return OperationResult.SuccessSync();
        }

        private UniTask<OperationResult> _dispose()
        {
            UnsubscribeToInputSchemeChange();

            if (IsHardwareInput)
            {
                UnsubscribeInputActions();
            }

            return OperationResult.SuccessSync();
        }

        private void _tick()
        {
            if (_input.Scheme is EInputScheme.Touch or EInputScheme.None)
            {
                return;
            }

            float2 rightStick = _input.Actions.UI.RightStick.ReadValue<Vector2>();
            if (!rightStick.Equals(float2.zero))
            {
                RightStickMoved?.Invoke(rightStick  * Time.deltaTime);
            }
        }

        public EInputScheme GetScheme()
        {
            return _input.Scheme;
        }

        private void SubscribeToInputSchemeChange()
        {
            _input.ChangedScheme += InputOnChangedScheme;
        }

        private void UnsubscribeToInputSchemeChange()
        {
            _input.ChangedScheme -= InputOnChangedScheme;
        }

        private void InputOnChangedScheme(EInputScheme scheme)
        {
            switch (IsHardwareInput)
            {
                case false when scheme is not EInputScheme.Touch:
                    IsHardwareInput = true;
                    SubscribeInputActions();
                    ChangedEnable?.Invoke();
                    break;
                case true when scheme is EInputScheme.Touch:
                    IsHardwareInput = false;
                    UnsubscribeInputActions();
                    ChangedEnable?.Invoke();
                    break;
            }
        }

        private void SubscribeInputActions()
        {
            _input.Actions.UI.Enable();
            _input.Actions.UI.Cancel.started += OnPressPause;
            _input.Actions.UI.Select.started += OnPressSelect;
            _input.Actions.UI.NorthButton.started += OnPressNorth;
            _input.Actions.UI.EastButton.started += OnPressEast;
            _input.Actions.UI.WestButton.started += OnPressWest;
            _input.Actions.UI.SouthButton.started += OnPressSouth;
            _input.Actions.UI.Navigate.performed += OnPerformedNavigate;
            _input.Actions.UI.Trigger.started += OnPerformedTrigger;
            _input.Actions.UI.Shoulder.started += OnPerformedShoulder;
        }

        private void UnsubscribeInputActions()
        {
            _input.Actions.UI.Disable();
            _input.Actions.UI.Cancel.started -= OnPressPause;
            _input.Actions.UI.Select.started -= OnPressSelect;
            _input.Actions.UI.NorthButton.started -= OnPressNorth;
            _input.Actions.UI.EastButton.started -= OnPressEast;
            _input.Actions.UI.WestButton.started -= OnPressWest;
            _input.Actions.UI.SouthButton.started -= OnPressSouth;
            _input.Actions.UI.Navigate.performed -= OnPerformedNavigate;
            _input.Actions.UI.Trigger.started -= OnPerformedTrigger;
            _input.Actions.UI.Shoulder.started -= OnPerformedShoulder;
        }

        private void OnPressSouth(InputAction.CallbackContext obj)
        {
            ActionButtonPressed?.Invoke(ESelectionZone.SouthButton);
        }

        private void OnPressWest(InputAction.CallbackContext obj)
        {
            ActionButtonPressed?.Invoke(ESelectionZone.WestButton);
        }

        private void OnPressEast(InputAction.CallbackContext obj)
        {
            ActionButtonPressed?.Invoke(ESelectionZone.EastButton);
        }

        private void OnPressNorth(InputAction.CallbackContext obj)
        {
            ActionButtonPressed?.Invoke(ESelectionZone.NorthButton);
        }

        private void OnPressSelect(InputAction.CallbackContext obj)
        {
            SelectButtonPressed?.Invoke();    
        }

        private void OnPressPause(InputAction.CallbackContext obj)
        {
            PauseButtonPressed?.Invoke();
        }

        private void OnPerformedNavigate(InputAction.CallbackContext obj)
        {
            float2 value = obj.ReadValue<Vector2>();
            int2 roundValue = RoundValue(value);

            if (roundValue.Equals(int2.zero))
            {
                return;
            }

            DpadPressed?.Invoke(roundValue);
        }

        private void OnPerformedShoulder(InputAction.CallbackContext obj)
        {
            float value = obj.ReadValue<float>();
            ShoulderPressed?.Invoke(RoundValue(value));
        }

        private void OnPerformedTrigger(InputAction.CallbackContext obj)
        {
            float value = obj.ReadValue<float>();
            TriggerPressed?.Invoke(RoundValue(value));
        }

        private int2 RoundValue(float2 vector)
        {
            return new int2(RoundValue(vector.x), RoundValue(vector.y));
        }

        private int RoundValue(float value)
        {
            return value switch
            {
                > 0.8f => BMath.CeilToInt(value),
                < -0.8f => BMath.FloorToInt(value),
                _ => 0
            };
        }
    }
}