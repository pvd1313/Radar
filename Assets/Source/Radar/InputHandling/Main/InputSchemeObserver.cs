using System;
using Beast.Claw.Debug;
using UnityEngine.InputSystem;

namespace Beast.Radar.InputHandling
{
    public class InputSchemeObserver : IInputSchemeObserver
    {
        public event Action<EInputScheme> Changed;
        
        public bool IsHardwareInput { get; private set; }
        public EInputScheme Current => _inputScheme;
        
        private readonly PlayerInputView _view;
        private EInputScheme _inputScheme;
        
        public InputSchemeObserver(PlayerInputView view)
        {
            _view = view;
        }

        public void Warmup()
        {
            _view.playerInput.onControlsChanged += _refreshScheme;
            _refreshScheme(_view.playerInput);
        }
        
        public void Dispose()
        {
            _view.playerInput.onControlsChanged -= _refreshScheme;
        }

        private void _refreshScheme(PlayerInput input)
        {
            Enum.TryParse(_view.playerInput.currentControlScheme, out _inputScheme);
            
            if (_inputScheme == EInputScheme.MouseAndKeyboard && Mouse.current == null)
            {
                BDebug.LogError(this,"Try setting the mouse scheme with an empty mouse device.");
                _inputScheme = EInputScheme.Touch;
            }
            
            switch (IsHardwareInput)
            {
                case false when _inputScheme is not EInputScheme.Touch:
                    IsHardwareInput = true;
                    break;

                case true when _inputScheme is EInputScheme.Touch:
                    IsHardwareInput = false;
                    break;
            }
            
            Changed?.Invoke(_inputScheme);
        }
    }
}