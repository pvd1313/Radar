using System;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Beast.Radar.Cinematic
{
    public class PlayerInputView : BeastMono, Initializable, Disposable
    {
        public EInputScheme Scheme => _inputScheme;

        private EInputScheme _inputScheme;
        public PlayerInput PlayerInput => _playerInput;
        
        [SerializeField] private PlayerInput _playerInput;
        
        public UniTask<InitializationResult> Initialize()
        {
            RefreshCurrentScheme(_playerInput);
            SubscribeToEvents(true);
            return InitializationResult.SuccessNotAsync();
        }

        public UniTask<DispositionResult> Dispose()
        {
            SubscribeToEvents(false);
            return DispositionResult.SuccessNotAsync();
        }

        private void SubscribeToEvents(bool flag)
        {
            if (flag)
            {
                _playerInput.onControlsChanged += RefreshCurrentScheme;
            }
            else
            {
                _playerInput.onControlsChanged -= RefreshCurrentScheme;
            }
        }
        
        private void RefreshCurrentScheme(PlayerInput input)
        {
            Enum.TryParse(_playerInput.currentControlScheme, out  _inputScheme);
            
            if (_inputScheme == EInputScheme.Mouse && Mouse.current == null)
            {
                BeastLog.Error(this,"Try setting the mouse scheme with an empty mouse device.");
                _inputScheme = EInputScheme.Touch;
                return;
            }
            
            Debug.Log($"Current input scheme is being refreshed: {_inputScheme}");
        }
    }
}