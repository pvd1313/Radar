using System;
using Beast.Claw.Debug;
using Beast.Claw.Mathematics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Beast.Radar.InputHandling
{
    public class UIInputActions : IUIInputActions
    {
        public event Action ChangedEnable;
        public InputValueControl<EInputOptionButton> OptionButtons { get; } = new();
        public InputValueControl<int> Trigger { get; } = new();
        public InputValueControl<int> Shoulder { get; } = new();
        public InputValueControl<int2> MainNavigation { get; } = new();
        public InputValueControl<float2> OptionNavigation { get; } = new();

        private readonly IInputSchemeObserver _inputScheme;
        private readonly InputActions _inputActions;

        private bool _isEnabled;
        private int2 _lastRightStickValue;
        private const float PRESS_COOLDOWN = 0.2f;
        private float _currentNavigationCooldown;

        public UIInputActions(IInputSchemeObserver inputScheme, InputActions inputActions)
        {
            _inputScheme = inputScheme;
            _inputActions = inputActions;
        }
        
        public void Warmup()
        {
            _subscribeToInputSchemeChange();

            if (_inputScheme.Current is not EInputScheme.Touch)
            {
                _subscribeInputActions();
            }
        }

        public void Update()
        {
            if (_inputScheme.Current is EInputScheme.Touch or EInputScheme.None)
            {
                return;
            }

            _performOptionNavigation();
            _performNavigate();
        }

        public void Dispose()
        {
            _unsubscribeToInputSchemeChange();

            if (_inputScheme.IsHardwareInput)
            {
                _unsubscribeInputActions();
            }
        }

        private void _performOptionNavigation()
        {
            float2 rightStick = (float2)_inputActions.UI.RightStick.ReadValue<Vector2>();
            OptionNavigation.UpdateValue(rightStick);
        }

        private void _performNavigate()
        {
            float2 value = (float2)_inputActions.UI.Navigate.ReadValue<Vector2>();
            MainNavigation.UpdateValue(int2.zero);
           
            if (!value.Equals(float2.zero))
            {
                if (_currentNavigationCooldown == 0)
                {
                    MainNavigation.UpdateValue(_roundValue(value));
                }

                _currentNavigationCooldown += Time.deltaTime;
                if ( _currentNavigationCooldown >= PRESS_COOLDOWN)
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
            _inputScheme.Changed += _onChangedScheme;
        }

        private void _unsubscribeToInputSchemeChange()
        {
            _inputScheme.Changed -= _onChangedScheme;
        }

        private void _onChangedScheme(EInputScheme scheme)
        {
            switch (_inputScheme.IsHardwareInput)
            {
                case false when _isEnabled:
                    _isEnabled = false;
                    _unsubscribeInputActions();
                    ChangedEnable?.Invoke();
                    break;

                case true when !_isEnabled:
                    _isEnabled = true;
                    _subscribeInputActions();
                    ChangedEnable?.Invoke();
                    break;
            }
        }

        private void _subscribeInputActions()
        {
            _inputActions.UI.Cancel.started += _ => OptionButtons.UpdateValue(EInputOptionButton.Start);
            _inputActions.UI.Cancel.canceled += _ => OptionButtons.UpdateValue(EInputOptionButton.None);
            
            _inputActions.UI.Select.started += _ => OptionButtons.UpdateValue(EInputOptionButton.Back);
            _inputActions.UI.Select.canceled += _ => OptionButtons.UpdateValue(EInputOptionButton.None);

            _inputActions.UI.SouthButton.started += _ => OptionButtons.UpdateValue(EInputOptionButton.First);
            _inputActions.UI.SouthButton.canceled += _ => OptionButtons.UpdateValue(EInputOptionButton.None);
            
            _inputActions.UI.WestButton.started += _ => OptionButtons.UpdateValue(EInputOptionButton.Second);
            _inputActions.UI.WestButton.canceled += _ => OptionButtons.UpdateValue(EInputOptionButton.None);
            
            _inputActions.UI.NorthButton.started += _ => OptionButtons.UpdateValue(EInputOptionButton.Third);
            _inputActions.UI.NorthButton.canceled += _ => OptionButtons.UpdateValue(EInputOptionButton.None);
            
            _inputActions.UI.EastButton.started += _ => OptionButtons.UpdateValue(EInputOptionButton.Fourth);
            _inputActions.UI.EastButton.canceled += _ => OptionButtons.UpdateValue(EInputOptionButton.None);
            
            _inputActions.UI.Trigger.started += _onPerformedTrigger;
            _inputActions.UI.Shoulder.started += _onPerformedShoulder;
        }

        private void _unsubscribeInputActions()
        {
            _inputActions.UI.Trigger.started -= _onPerformedTrigger;
            _inputActions.UI.Shoulder.started -= _onPerformedShoulder;
        }


        private void _onPerformedShoulder(InputAction.CallbackContext obj)
        {
            float value = obj.ReadValue<float>();
            BDebug.LogInfo(this, $"shoulder {value}");
            Shoulder.UpdateValue(_roundValue(value));
            Shoulder.EndPress(0);
        }

        private void _onPerformedTrigger(InputAction.CallbackContext obj)
        {
            float value = obj.ReadValue<float>();
            BDebug.LogInfo(this, $"trigger {value}");
            Trigger.UpdateValue(_roundValue(value, 0.2f));
            Trigger.EndPress(0);
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
    }
}