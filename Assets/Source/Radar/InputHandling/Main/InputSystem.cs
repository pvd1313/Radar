using System;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine.InputSystem;

namespace Beast.Radar.InputControl
{
    public class InputSystem : IInputSystem, IGameCycleListener
    {
        public event Action<EInputScheme> ChangedScheme;

        public IGameActionMap GameActionMap => _gameActionMap;
        public InputActions Actions { get; private set; }
        public EInputScheme Scheme => _inputScheme;
        public float ClickDuration => 0.1f;

        private EInputScheme _inputScheme;
        private PlayerInputView _view;
        private GameActionMap _gameActionMap;
        
        public InputSystem(PrefabLibrary config)
        {
            _view = config.playerInputPrefab;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(OperationID.WARMUP, _warmup);
            handle.ListenUpdate(UpdateID.TICK, _tick);
            handle.ListenOperation(OperationID.DISPOSE, _dispose);
        }

        private UniTask<OperationResult> _warmup()
        {
            Actions = new InputActions();
            Actions.Enable();
            Actions.UI.Enable();
            Actions.Player.Enable();

            PlayerInputView newInstance = BScene.InstantiateMono(this, _view, float3.zero, quaternion.identity);
            _view = newInstance;
            _refreshScheme(_view.playerInput);
            _view.playerInput.onControlsChanged += _refreshScheme;

            _gameActionMap = new GameActionMap(this, Actions);
            _gameActionMap.Warmup();
            
            return OperationResult.SuccessSync();
        }

        private void _tick()
        {
            
            _gameActionMap.Update();
            
        }

        private void _refreshScheme(PlayerInput input)
        {
            Enum.TryParse(_view.playerInput.currentControlScheme, out _inputScheme);
            
            if (_inputScheme == EInputScheme.MouseAndKeyboard && Mouse.current == null)
            {
                BDebug.LogError(this,"Try setting the mouse scheme with an empty mouse device.");
                _inputScheme = EInputScheme.Touch;
            }
            
            ChangedScheme?.Invoke(_inputScheme);
        }

        private UniTask<OperationResult> _dispose()
        {
            _view.playerInput.onControlsChanged -= _refreshScheme;
            Actions.Disable();
            Actions.UI.Disable();
            Actions.Player.Disable();
            _gameActionMap.Dispose();

            return OperationResult.SuccessSync();
        }
    }
}