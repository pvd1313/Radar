using System;
using Beast.Claw.Debug;
using UnityEngine.InputSystem;

namespace Beast.Radar.InputHandling
{
    public class InputSchemeObserver : IInputSchemeObserver
    {
        public event Action<EInputScheme> Changed;
        
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
            
            Changed?.Invoke(_inputScheme);
        }
    }
}