using System;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Beast.Radar.InputControl
{
    public class UIInputController : IUIInputController
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

        public UniTask<InitializationResult> Initialize()
        {
            Subscribe();
            return InitializationResult.SuccessNotAsync();
        }

        public void Dispose()
        {
            Unsubscribe();
        }

        public void OnFrameBegin()
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

        private void Subscribe()
        {
            _input.Actions.Player.Pause.started += OnStartedPause;
        }

        private void Unsubscribe()
        {
            _input.Actions.Player.Pause.started -= OnStartedPause;
        }

        private void OnStartedPause(InputAction.CallbackContext obj)
        {
            PausePressed?.Invoke();
        }
    }
}