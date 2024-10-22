using System;
using Beast.Claw.GameCycle;
using Beast.Radar.Cinematic;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

namespace Beast.Radar.InputControl
{
    public class PlayerInputController : IPlayerInputController, IGameCycleListener
    {
        public event Action<float2> PressMove;

        private readonly ICameraTransform _cameraTransform;
        private readonly IInputActionsProvider _inputActions;

        public PlayerInputController(ICameraTransform cameraTransform, IInputActionsProvider inputActions)
        {
            _cameraTransform = cameraTransform;
            _inputActions = inputActions;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(OperationID.WARMUP, _warmup);
            handle.ListenOperation(OperationID.DISPOSE, _dispose);
        }

        private UniTask<OperationResult> _warmup()
        {
            return OperationResult.SuccessSync();
        }

        private UniTask<OperationResult> _dispose()
        {
            return OperationResult.SuccessSync();
        }
    }
}