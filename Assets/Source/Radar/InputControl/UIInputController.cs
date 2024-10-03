using System;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;

namespace Beast.Radar.InputControl
{
    public class UIInputController : IUIInputController
    {
        private readonly IPlayerInputProvider _input;
        public event Action PausePressed;

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