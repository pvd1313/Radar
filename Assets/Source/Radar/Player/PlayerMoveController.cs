using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Math;
using Beast.Radar.Cinematic;
using Beast.Radar.InputHandling;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Player
{
    public class PlayerMoveController : IGameCycleListener
    {
        private readonly ICameraTransform _cameraTransform;
        private readonly PrefabLibrary _library;
        private readonly IPlayerInputController _input;

        private PlayerView _player;
        private int2 _currentTarget = new(512, 512);
        private float2 _currentCoordinate = new(512, 512);
        private float _speed = 25;

        public PlayerMoveController(
            ICameraTransform cameraTransform,
            PrefabLibrary library,
            IPlayerInputController input)
        {
            _cameraTransform = cameraTransform;
            _library = library;
            _input = input;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(OperationID.WARMUP, _warmup);
            handle.ListenUpdate(UpdateID.TICK, _tick);
        }

        private UniTask<OperationResult> _warmup()
        {
            _spawn();
            _input.ClickMove += _onClickMove;
            return OperationResult.SuccessSync();
        }

        private void _spawn()
        {
            _player = BScene.InstantiateMono(this, _library.playerViewPrefab, float3.zero, quaternion.identity);
        }

        private void _tick()
        {
            _currentCoordinate = BMath.MoveTowards(_currentCoordinate, _currentTarget, _speed * Time.deltaTime);
            float2 worldPosition = _cameraTransform.WorldToScreen(_currentCoordinate);
            _player.body.position = new Vector3(worldPosition.x, worldPosition.y, 0);

            BDebug.Track(this, "current Target", _currentTarget);
            BDebug.Track(this, "current Coordinate", _currentCoordinate);
        }

        private void _onClickMove(float2 obj)
        {
            float2 worldClickPosition = _cameraTransform.ScreenToWorld(obj);
            _currentTarget = new int2((int)worldClickPosition.x, (int)worldClickPosition.y);

            Vector3 positionPosition = _player.body.position;
            float2 playerScreenPosition = new float2(positionPosition.x, positionPosition.y);
            _currentCoordinate = _cameraTransform.ScreenToWorld(playerScreenPosition);
        }
    }
}