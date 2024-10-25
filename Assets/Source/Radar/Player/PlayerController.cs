using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Beast.Radar.Cinematic;
using Beast.Radar.InputHandling;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Player
{
    public class PlayerController : IGameCycleListener
    {
        private readonly PrefabLibrary _library;
        private readonly PlayerConfig _playerConfig;
        private readonly ICameraTransform _cameraTransform;
        private readonly IInputSystem _input;

        private PlayerView _player;
        private int2 _currentTarget = new(512, 512);
        private float2 _currentCoordinate = new(512, 512);
        private float2 _startPressPosition;

        public PlayerController(
            ICameraTransform cameraTransform,
            PrefabLibrary library,
            PlayerConfig playerConfig,
            IInputSystem input)
        {
            _cameraTransform = cameraTransform;
            _library = library;
            _playerConfig = playerConfig;
            _input = input;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(OperationID.WARMUP, _warmup);
            handle.ListenUpdate(UpdateID.TICK, _tick);
            handle.ListenOperation(OperationID.DISPOSE, _dispose);
        }

        private UniTask<OperationResult> _warmup()
        {
            _spawnPlayer();
            _subscribeInput();

            return OperationResult.SuccessSync();
        }

        private void _tick()
        {
            _move();
        }

        private void _spawnPlayer()
        {
            _player = BScene.InstantiateMono(this, _library.playerViewPrefab, float3.zero, quaternion.identity);
        }

        private void _move()
        {
            _currentCoordinate = BMath.MoveTowards(
                _currentCoordinate,
                _currentTarget,
                _playerConfig.moveSpeed * Time.deltaTime);

            float2 worldPosition = _cameraTransform.WorldToScreen(_currentCoordinate);

            _player.body.position = new Vector3(worldPosition.x, worldPosition.y, 0);

            BDebug.Track(this, "current Target", _currentTarget);
            BDebug.Track(this, "current Coordinate", _currentCoordinate);
        }

        private void _subscribeInput()
        {
            _input.GameInput.Click.PressStarted += _onPressStarted;
            _input.GameInput.Click.PressCanceled += _onPressCanceled;
        }

        private void _unsubscribeInput()
        {
            _input.GameInput.Click.PressStarted -= _onPressStarted;
            _input.GameInput.Click.PressCanceled -= _onPressCanceled;
        }

        private void _onPressStarted(float2 position)
        {
            _startPressPosition = position;
        }

        private void _onPressCanceled(float2 position, float duration)
        {
            if (BMath.Distance(_startPressPosition, position) < 0.1f && duration < 0.2f)
            {
                float2 worldClickPosition = _cameraTransform.ScreenToWorld(position);
                
                _currentTarget = new int2((int)worldClickPosition.x, (int)worldClickPosition.y);

                Vector3 positionPosition = _player.body.position;
                float2 playerScreenPosition = new float2(positionPosition.x, positionPosition.y);
                
                _currentCoordinate = _cameraTransform.ScreenToWorld(playerScreenPosition);
            }
        }

        private UniTask<OperationResult> _dispose()
        {
            _unsubscribeInput();

            return OperationResult.SuccessSync();
        }
    }
}