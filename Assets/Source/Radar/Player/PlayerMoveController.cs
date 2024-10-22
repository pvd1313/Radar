using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Radar.Cinematic;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Player
{
    public class PlayerMoveController : IGameCycleListener
    {
        private readonly ICameraTransform _cameraTransform;
        private readonly PrefabLibrary _library;
        private PlayerView _player;

        private int2 _coordinate = new(512,512);

        public PlayerMoveController(
            ICameraTransform cameraTransform, 
            PrefabLibrary library)
        {
            _cameraTransform = cameraTransform;
            _library = library;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(OperationID.WARMUP, _warmup);
            handle.ListenUpdate(UpdateID.TICK, _tick);
        }

        private UniTask<OperationResult> _warmup()
        {
            _spawn();
            return OperationResult.SuccessSync();
        }

        private void _spawn()
        {
            _player = BScene.InstantiateMono(this, _library.playerViewPrefab, float3.zero, quaternion.identity);
        }

        private void _tick()
        {
            float2 worldPosition = _cameraTransform.WorldToScreen(_coordinate);
            _player.body.position = new Vector3(worldPosition.x, worldPosition.y);
        }
    }
}