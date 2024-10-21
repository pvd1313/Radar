using System;
using Beast.Claw.Debug;
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
        public event Action ChangedEnable;
        public event Action PauseButtonPressed;
        public event Action SelectButtonPressed;
        public event Action<ESelectionZone> ActionButtonPressed;
        public event Action<int2> NavigatePressed;
        public event Action<float2> RightStickMoved;
        public event Action<int> ShoulderPressed;
        public event Action<int> TriggerPressed;
        public bool IsHardwareInput { get; private set; }

        private readonly IPlayerInputProvider _input;

        private int2 _lastRightStickValue;
        private const float PRESS_COOLDOWN = 0.2f;
        private float _currentNavigationCooldown;

        public UIInputController(IPlayerInputProvider inputProvider)
        {
            _input = inputProvider;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.WARMUP, _warmup);
            handle.ListenOperation(this, OperationID.DISPOSE, _dispose);
            handle.ListenUpdate(this, UpdateID.TICK, _tick);
        }

        private UniTask<OperationResult> _warmup()
        {
            _subscribeToInputSchemeChange();

            IsHardwareInput = _input.Scheme is not EInputScheme.Touch;

            if (IsHardwareInput)
            {
                _subscribeInputActions();
            }

            return OperationResult.SuccessSync();
        }

        private void _tick()
        {
            if (_input.Scheme is EInputScheme.Touch or EInputScheme.None)
            {
                return;
            }

            _performRightStick();
            _performNavigate();
        }

        public EInputScheme GetScheme()
        {
            return _input.Scheme;
        }

        private void _performRightStick()
        {
            float2 rightStick = _input.Actions.UI.RightStick.ReadValue<Vector2>();
            if (!rightStick.Equals(float2.zero))
            {
                RightStickMoved?.Invoke(rightStick * Time.deltaTime);
            }
        }

        private void _performNavigate()
        {
            float2 value = _input.Actions.UI.Navigate.ReadValue<Vector2>();
            if (!value.Equals(float2.zero))
            {
                if (_currentNavigationCooldown == 0)
                {
                    NavigatePressed?.Invoke(_roundValue(value));
                }

                _currentNavigationCooldown += Time.deltaTime;
                if (_currentNavigationCooldown >= PRESS_COOLDOWN)
                {
                    _currentNavigationCooldown = 0;
                }
            }
            else
            {
                _currentNavigationCooldown = 0;
            }
        }

        private void _subscribeToInputSchemeChange()
        {
            _input.ChangedScheme += _onChangedScheme;
        }

        private void _unsubscribeToInputSchemeChange()
        {
            _input.ChangedScheme -= _onChangedScheme;
        }

        private void _onChangedScheme(EInputScheme scheme)
        {
            switch (IsHardwareInput)
            {
                case false when scheme is not EInputScheme.Touch:
                    IsHardwareInput = true;
                    _subscribeInputActions();
                    ChangedEnable?.Invoke();
                    break;

                case true when scheme is EInputScheme.Touch:
                    IsHardwareInput = false;
                    _unsubscribeInputActions();
                    ChangedEnable?.Invoke();
                    break;
            }
        }

        private void _subscribeInputActions()
        {
            _input.Actions.UI.Enable();
            _input.Actions.UI.Cancel.started += _onPressPause;
            _input.Actions.UI.Select.started += _onPressSelect;
            _input.Actions.UI.NorthButton.started += _onPressNorth;
            _input.Actions.UI.EastButton.started += _onPressEast;
            _input.Actions.UI.WestButton.started += _onPressWest;
            _input.Actions.UI.SouthButton.started += _onPressSouth;
            _input.Actions.UI.Trigger.started += _onPerformedTrigger;
            _input.Actions.UI.Shoulder.started += _onPerformedShoulder;
        }

        private void _unsubscribeInputActions()
        {
            _input.Actions.UI.Disable();
            _input.Actions.UI.Cancel.started -= _onPressPause;
            _input.Actions.UI.Select.started -= _onPressSelect;
            _input.Actions.UI.NorthButton.started -= _onPressNorth;
            _input.Actions.UI.EastButton.started -= _onPressEast;
            _input.Actions.UI.WestButton.started -= _onPressWest;
            _input.Actions.UI.SouthButton.started -= _onPressSouth;
            _input.Actions.UI.Trigger.started -= _onPerformedTrigger;
            _input.Actions.UI.Shoulder.started -= _onPerformedShoulder;
        }

        private void _onPressSouth(InputAction.CallbackContext obj)
        {
            ActionButtonPressed?.Invoke(ESelectionZone.SouthButton);
        }

        private void _onPressWest(InputAction.CallbackContext obj)
        {
            ActionButtonPressed?.Invoke(ESelectionZone.WestButton);
        }

        private void _onPressEast(InputAction.CallbackContext obj)
        {
            ActionButtonPressed?.Invoke(ESelectionZone.EastButton);
        }

        private void _onPressNorth(InputAction.CallbackContext obj)
        {
            ActionButtonPressed?.Invoke(ESelectionZone.NorthButton);
        }

        private void _onPressSelect(InputAction.CallbackContext obj)
        {
            SelectButtonPressed?.Invoke();
        }

        private void _onPressPause(InputAction.CallbackContext obj)
        {
            PauseButtonPressed?.Invoke();
        }

        private void _onPerformedShoulder(InputAction.CallbackContext obj)
        {
            float value = obj.ReadValue<float>();
            ShoulderPressed?.Invoke(_roundValue(value));
        }

        private void _onPerformedTrigger(InputAction.CallbackContext obj)
        {
            float value = obj.ReadValue<float>();
            TriggerPressed?.Invoke(_roundValue(value, 0.2f));
        }

        private int2 _roundValue(float2 vector)
        {
            return new int2(_roundValue(vector.x), _roundValue(vector.y));
        }

        private int _roundValue(float value, float threshold = 0.8f)
        {
            return value switch
            {
                _ when value > threshold => BMath.CeilToInt(value),
                _ when value <-threshold => BMath.FloorToInt(value),
                _ => 0
            };
        }

        private UniTask<OperationResult> _dispose()
        {
            _unsubscribeToInputSchemeChange();

            if (IsHardwareInput)
            {
                _unsubscribeInputActions();
            }

            return OperationResult.SuccessSync();
        }
    }
}