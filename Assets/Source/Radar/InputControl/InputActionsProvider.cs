using System;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine.InputSystem;

namespace Beast.Radar.InputControl
{
    public class InputActionsProvider : IInputActionsProvider, IGameCycleListener
    {
        public event Action<EInputScheme> ChangedScheme;
        
        public InputActions Actions { get; private set; }
        public EInputScheme Scheme => _inputScheme;
        public float ClickDuration => 0.1f;

        private EInputScheme _inputScheme;
        private PlayerInputView _view;
        
        public InputActionsProvider(PrefabLibrary config)
        {
            _view = config.playerInputPrefab;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(OperationID.WARMUP, Warmup);
            handle.ListenOperation(OperationID.DISPOSE, Dispose);
        }
        
        private UniTask<OperationResult> Warmup()
        {
            Actions = new InputActions();
            Actions.Enable();
            Actions.UI.Enable();
            Actions.Player.Enable();

            PlayerInputView newInstance = BScene.InstantiateMono(this, _view, float3.zero, quaternion.identity);
            _view = newInstance;
            RefreshCurrentScheme(_view.playerInput);
            _view.playerInput.onControlsChanged += RefreshCurrentScheme;
            
            return OperationResult.SuccessSync();
        }

        private UniTask<OperationResult> Dispose()
        {
            _view.playerInput.onControlsChanged -= RefreshCurrentScheme;
            Actions.Disable();
            Actions.UI.Disable();
            Actions.Player.Disable();

            return OperationResult.SuccessSync();
        }
        
        private void RefreshCurrentScheme(PlayerInput input)
        {
            Enum.TryParse(_view.playerInput.currentControlScheme, out _inputScheme);
            
            if (_inputScheme == EInputScheme.MouseAndKeyboard && Mouse.current == null)
            {
                BDebug.LogError(this,"Try setting the mouse scheme with an empty mouse device.");
                _inputScheme = EInputScheme.Touch;
            }
            
            ChangedScheme?.Invoke(_inputScheme);
        }
    }
}