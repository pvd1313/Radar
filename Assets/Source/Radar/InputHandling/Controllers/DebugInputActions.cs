using UnityEngine.InputSystem;

namespace Beast.Radar.InputHandling
{
    public class DebugInputActions : IDebugInputActions
    {
        private readonly InputActions _inputActions;
        public InputControl TeleportButton { get; } = new();

        public DebugInputActions(InputActions inputAction)
        {
            _inputActions = inputAction;
        }

        public void Warmup()
        {
            _inputActions.Game.DebugButton.started += _onPressDebug;
            _inputActions.Game.DebugButton.started += _onStoppedPressDebug;
        }
        
        private void _onPressDebug(InputAction.CallbackContext obj)
        {
            TeleportButton.StartPress();
        }

        private void _onStoppedPressDebug(InputAction.CallbackContext obj)
        {
            TeleportButton.EndPress();
        }
    }
}