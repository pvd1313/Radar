using System;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Beast.Radar.InputControl
{
    public class BeastUIInputController : IBeastUIInputController, IBeastGameCycleListener
    {
        private const float NavigationCooldown = 0.2f;
        public event Action<float2> NavigationPressed;
        public event Action PausePressed;

        private readonly IBeastPlayerInputProvider _input;

        private float _currentNavigationCooldown;
   
        public BeastUIInputController(IBeastPlayerInputProvider inputProvider)
        {
            _input = inputProvider;
        }
        
        public void ListenGameCycle(IBeastGameCycleHandle handle)
        {
            handle.ListenOperation(this, BeastCycleEventID.Warmup, Warmup);
            handle.ListenOperation(this, BeastCycleEventID.Dispose, Dispose);
            handle.ListenUpdate(this, BeastCycleEventID.FrameBegin, FrameBegin);
        }

        private UniTask<BeastInitializationResult> Warmup()
        {
            SubscribeInputActions();

            return BeastInitializationResult.SuccessSync();
        }

        private UniTask<BeastInitializationResult> Dispose()
        {
            UnsubscribeInputActions();

            return BeastInitializationResult.SuccessSync();
        }

        private void FrameBegin()
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