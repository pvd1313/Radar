using System;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Beast.Radar.Cinematic
{
    public class PlayerInputView: BeastMono, Initializable, Disposable
    {
        public EInputScheme Scheme => _inputScheme;

        private EInputScheme _inputScheme;
        
        [SerializeField] private PlayerInput _playerInput;
        
        public UniTask<InitializationResult> Initialize()
        {
            RefreshCurrentScheme();
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
                _playerInput.onControlsChanged += OnControlsChanged;
            }
            else
            {
                _playerInput.onControlsChanged -= OnControlsChanged;
            }
        }

        private void OnControlsChanged(PlayerInput playerInput)
        {
            RefreshCurrentScheme(); 
            Debug.Log($"On Change player input scheme {_inputScheme}");
        }

        private void RefreshCurrentScheme()
        {
            Enum.TryParse(_playerInput.currentControlScheme, out  _inputScheme);
        }
    }
}