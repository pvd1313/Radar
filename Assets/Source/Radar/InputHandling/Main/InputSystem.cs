using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

namespace Beast.Radar.InputHandling
{
    public class InputSystem : IInputSystem, IGameCycleListener
    {
        public IInputSchemeObserver Scheme => _schemeObserver;
        public IGameInputActions GameInput => _gameInputActions;
        public IUIInputActions UIInput => _uiInputActions;
        public float ClickDuration => 0.1f;

        private readonly PlayerInputView _view;
        
        private InputSchemeObserver _schemeObserver;
        private GameInputActions _gameInputActions;
        private UIInputActions _uiInputActions;
        private InputActions _actions;


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

        private  UniTask<OperationResult> _warmup()
        {
            _actions = new InputActions();
            _actions.Enable();
            _actions.UI.Enable();
            _actions.Player.Enable();
            
            PlayerInputView playerInputView = BScene.InstantiateMono(this, _view, float3.zero, quaternion.identity);
            _schemeObserver = new InputSchemeObserver(playerInputView);
            _schemeObserver.Warmup();

            _uiInputActions = new UIInputActions(Scheme, _actions);
            _uiInputActions.Warmup();
            
            _gameInputActions = new GameInputActions(Scheme, _actions);
            
            
            return OperationResult.SuccessSync();
        }

        private void _tick()
        {
            _gameInputActions.Update();
            _uiInputActions.Update();
        }

        private UniTask<OperationResult> _dispose()
        {
            _uiInputActions.Dispose();
            _schemeObserver.Dispose();
            _actions.Disable();
            _actions.UI.Disable();
            _actions.Player.Disable();

            return OperationResult.SuccessSync();
        }
    }
}