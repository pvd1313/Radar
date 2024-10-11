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
        private const float NavigationCooldown = 0.2f;
        public event Action<float2> NavigationPressed;
        public event Action PausePressed;

        private readonly IPlayerInputProvider _input;

        private float _currentNavigationCooldown;
   
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
            SubscribeInputActions();

            return OperationResult.SuccessSync();
        }

        private UniTask<OperationResult> _dispose()
        {
            UnsubscribeInputActions();

            return OperationResult.SuccessSync();
        }

        private void _tick()
        {
            Navigate();
        }

        private void Navigate()
        {
            float2 vector = _input.Actions.UI.Navigate.ReadValue<Vector2>();
            bool isNavigation = !vector.Equals(float2.zero);
            if (isNavigation)
            {
                if (_currentNavigationCooldown > 0)
                {
                    _currentNavigationCooldown -= Time.deltaTime;
                }
                else
                {
                    NavigationPressed?.Invoke(vector * -1);
                    _currentNavigationCooldown = NavigationCooldown;
                }
            }
            else if (_currentNavigationCooldown != 0)
            {
                _currentNavigationCooldown = 0;
            }
        }

        private void SubscribeInputActions()
        {
            _input.Actions.Player.Pause.started += OnStartedPause;
        }

        private void UnsubscribeInputActions()
        {
            _input.Actions.Player.Pause.started -= OnStartedPause;
        }

        private void OnStartedPause(InputAction.CallbackContext obj)
        {
            PausePressed?.Invoke();
        }
    }
}