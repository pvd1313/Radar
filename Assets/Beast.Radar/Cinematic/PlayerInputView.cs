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
            _playerInput = FindObjectOfType<PlayerInput>();
            RefreshCurrentScheme(_playerInput);
            SubscribeToEvents(true);
            return InitializationResult.SuccessNotAsync();
        }

        public UniTask<DispositionResult> Dispose()
        {
            SubscribeToEvents(false);
            return DispositionResult.SuccessNotAsync();
        }

        private void Update()
        {
            Debug.Log("scheme " +_playerInput.currentControlScheme);
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
            RefreshCurrentScheme(playerInput); 
            Debug.Log($"On Change player input scheme {_inputScheme}");
        }

        private void RefreshCurrentScheme(PlayerInput input)
        {
            Enum.TryParse(input.currentControlScheme, out  _inputScheme);
            Debug.Log($"Refresh current scheme {_inputScheme}");
        }
    }
}