using System;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine.InputSystem;

namespace Beast.Radar.InputControl
{
    public class BeastPlayerInputProvider : IBeastPlayerInputProvider 
    {
        public InputActions Actions { get; private set; }
        public event Action<EBeastInputScheme> ChangedScheme;
        public EBeastInputScheme Scheme => _inputScheme;

        private EBeastInputScheme _inputScheme;
        private BeastPlayerInputView _view;
        
        public BeastPlayerInputProvider(BeastPrefabLibrary config)
        {
            _view = config.playerInputPrefab;
        }
        
        public UniTask<BeastInitializationResult> Initialize()
        {
            Actions = new InputActions();
            Actions.Enable();
            
            BeastPlayerInputView newInstance = BeastScene.InstantiateMono(this, _view, float3.zero, quaternion.identity);
            _view = newInstance;
            RefreshCurrentScheme(_view.playerInput);
            _view.playerInput.onControlsChanged += RefreshCurrentScheme;
            
            return BeastInitializationResult.SuccessNotAsync();
        }

        public void Dispose()
        {
            _view.playerInput.onControlsChanged -= RefreshCurrentScheme;
            Actions.Disable();
        }
        
        private void RefreshCurrentScheme(PlayerInput input)
        {
            Enum.TryParse(_view.playerInput.currentControlScheme, out  _inputScheme);
            
            if (_inputScheme == EBeastInputScheme.MouseAndKeyboard && Mouse.current == null)
            {
                BDebug.LogError(this,"Try setting the mouse scheme with an empty mouse device.");
                _inputScheme = EBeastInputScheme.Touch;
            }
            
            ChangedScheme?.Invoke(_inputScheme);
        }
    }
}