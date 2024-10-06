using System;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine.InputSystem;

namespace Beast.Radar.InputControl
{
    public class BeastPlayerInputProvider : IBeastPlayerInputProvider, IBeastGameCycleListener
    {
        public event Action<EBeastInputScheme> ChangedScheme;
        
        public InputActions Actions { get; private set; }
        public EBeastInputScheme Scheme => _inputScheme;

        private EBeastInputScheme _inputScheme;
        private BeastPlayerInputView _view;
        
        public BeastPlayerInputProvider(BeastPrefabLibrary config)
        {
            _view = config.playerInputPrefab;
        }
        
        public void ListenGameCycle(IBeastGameCycleHandle handle)
        {
            handle.ListenOperation(this, BeastCycleEventID.Warmup, Warmup);
            handle.ListenOperation(this, BeastCycleEventID.Dispose, Dispose);
        }
        
        private UniTask<BeastInitializationResult> Warmup()
        {
            Actions = new InputActions();
            Actions.Enable();
            
            BeastPlayerInputView newInstance = BeastScene.InstantiateMono(this, _view, float3.zero, quaternion.identity);
            _view = newInstance;
            RefreshCurrentScheme(_view.playerInput);
            _view.playerInput.onControlsChanged += RefreshCurrentScheme;
            
            return BeastInitializationResult.SuccessSync();
        }

        private UniTask<BeastInitializationResult> Dispose()
        {
            _view.playerInput.onControlsChanged -= RefreshCurrentScheme;
            Actions.Disable();

            return BeastInitializationResult.SuccessSync();
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