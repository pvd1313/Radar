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
        
        public InputSystem(CorePrefabLibrary config)
        {
            _view = config.playerInput;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
            handle.ListenBlocking(ActionID.TICK, _tick);
            handle.ListenBlocking(ActionID.DISPOSE, _dispose);
        }

        private UniTask _warmup(ConcurrentContext context)
        {
            _actions = new InputActions();
            _actions.Enable();
            _actions.UI.Enable();
            _actions.Game.Enable();
            
            PlayerInputView playerInputView = BScene.InstantiateMono(this, _view, float3.zero, quaternion.identity);
            _schemeObserver = new InputSchemeObserver(playerInputView);
            _schemeObserver.Warmup();

            _uiInputActions = new UIInputActions(Scheme, _actions);
            _uiInputActions.Warmup();
            
            _gameInputActions = new GameInputActions(Scheme, _actions);
            
            return UniTask.CompletedTask;
        }

        private void _tick(BlockingContext context)
        {
            _gameInputActions.Update();
            _uiInputActions.Update();
        }

        private void _dispose(BlockingContext context)
        {
            _uiInputActions.Dispose();
            _schemeObserver.Dispose();
            _actions.Disable();
            _actions.UI.Disable();
            _actions.Game.Disable();
        }
    }
}