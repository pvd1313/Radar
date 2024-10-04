using System;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine.InputSystem;

namespace Beast.Radar.InputControl
{
    public class PlayerInputProvider : IPlayerInputProvider 
    {
        public InputActions Actions { get; private set; }
        public event Action<EInputScheme> ChangedScheme;
        public EInputScheme Scheme => _inputScheme;

        private EInputScheme _inputScheme;
        private PlayerInputView _view;
        
        public PlayerInputProvider(BeastPrefabLibrary config)
        {
            _view = config.playerInputPrefab;
        }
        
        public UniTask<InitializationResult> Initialize()
        {
            Actions = new InputActions();
            Actions.Enable();
            
            PlayerInputView newInstance = BeastScene.InstantiateMono(this, _view, float3.zero, quaternion.identity);
            _view = newInstance;
            RefreshCurrentScheme(_view.PlayerInput);
            _view.PlayerInput.onControlsChanged += RefreshCurrentScheme;
            
            return InitializationResult.SuccessNotAsync();
        }

        public void Dispose()
        {
            _view.PlayerInput.onControlsChanged -= RefreshCurrentScheme;
            Actions.Disable();
        }
        
        private void RefreshCurrentScheme(PlayerInput input)
        {
            Enum.TryParse(_view.PlayerInput.currentControlScheme, out  _inputScheme);
            
            if (_inputScheme == EInputScheme.MouseAndKeyboard && Mouse.current == null)
            {
                BeastLog.Error(this,"Try setting the mouse scheme with an empty mouse device.");
                _inputScheme = EInputScheme.Touch;
            }
            
            ChangedScheme?.Invoke(_inputScheme);
        }
    }
}