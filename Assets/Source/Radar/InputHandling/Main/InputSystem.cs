using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

namespace Beast.Radar.InputHandling
{
    public class InputSystem : IInputSystem, IGameCycleListener
    {
        public IInputSchemeObserver Scheme => _schemeObserver;
        public IGameInputActions GameInput => _gameInputActions;
        public InputActions Actions { get; private set; }
        public float ClickDuration => 0.1f;

        private InputSchemeObserver _schemeObserver;
        private GameInputActions _gameInputActions;

        private EInputScheme _inputScheme;
        private PlayerInputView _view;
        
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

        private async UniTask<OperationResult> _warmup()
        {
            Actions = new InputActions();
            Actions.Enable();
            Actions.UI.Enable();
            Actions.Player.Enable();
            
            PlayerInputView playerInputView = BScene.InstantiateMono(this, _view, float3.zero, quaternion.identity);
            _schemeObserver = new InputSchemeObserver(playerInputView);
            _schemeObserver.Warmup();

            _gameInputActions = new GameInputActions(Scheme, Actions);
            await  UniTask.Yield();
            
            return OperationResult.Success();
        }

        private void _tick()
        {
            _gameInputActions.Update();
        }

        private UniTask<OperationResult> _dispose()
        {
            _schemeObserver.Dispose();
            Actions.Disable();
            Actions.UI.Disable();
            Actions.Player.Disable();

            return OperationResult.SuccessSync();
        }
    }
}