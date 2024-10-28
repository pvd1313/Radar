using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Gizmos;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Beast.Radar.GameTime;
using Beast.Radar.Graphic;
using Beast.Radar.InputHandling;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Player
{
    public class PlayerController : IGameCycleListener
    {
        private readonly PrefabLibrary _library;
        private readonly PlayerConfig _playerConfig;
        private readonly GameModel _game;
        private readonly IWorldTime _worldTime;
        private readonly ICameraTransform _cameraTransform;
        private readonly IInputSystem _input;
        private readonly IGizmosManager _gizmosManager;

        private PlayerView _player;

        private float2 _currentTarget = new(512, 512);
        private float2 _currentPosition = new(512, 512);
        private int2 _cellIndex;


        private float2 _startPressPosition;
        private int _texelSize;
        private GizmosHandle _gizmoHandle;
        private float _halfTexelSize;
        private float _lastRecordedGameHours;


        public PlayerController(
            ICameraTransform cameraTransform,
            PrefabLibrary library,
            PlayerConfig playerConfig,
            IInputSystem input,
            IGizmosManager gizmosManager,
            GameModel game,
            IWorldTime worldTime)
        {
            _cameraTransform = cameraTransform;
            _library = library;
            _playerConfig = playerConfig;
            _input = input;
            _gizmosManager = gizmosManager;
            _game = game;
            _worldTime = worldTime;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(ActionID.WARMUP, _warmup);
            handle.ListenUpdate(ActionID.TICK, _tick);
            handle.ListenOperation(ActionID.DISPOSE, _dispose);
            handle.ListenOperation(ActionID.GAME_LOAD, _gameLoad);
            handle.ListenOperation(ActionID.GAME_SAVE, _gameSave);
        }

        private UniTask<ActionResult> _warmup(ActionContext context)
        {
            _halfTexelSize = (1f / WorldTransform.CellTileCount) / 2.0f;
            
            _gizmoHandle = _gizmosManager.GetHandle(this, "cell points", Color.red);

            _subscribeInput();

            return ActionResult.SuccessSyncronous();
        }

        private void _tick(ActionContext context)
        {
            _move();
        }

        private UniTask<ActionResult> _gameLoad(ActionContext arg)
        {
            _spawnPlayer();

            return ActionResult.SuccessSyncronous();
        }

        private UniTask<ActionResult> _gameSave(ActionContext arg)
        {
            _game.player.position = _cellIndex;

            return ActionResult.SuccessSyncronous();
        }

        private void _subscribeInput()
        {
            _input.GameInput.Click.PressStarted += _onPressStarted;
            _input.GameInput.Click.PressCanceled += _onPressCanceled;
        }

        private void _spawnPlayer()
        {
            _currentPosition = _indexToWorld(_game.player.position);
            _currentTarget = _currentPosition;
            float2 screenPosition = _cameraTransform.WorldToScreen(_currentPosition);
            float3 spawnPosition = new(screenPosition.x, screenPosition.y, 0);

            if (_player == null)
            {
                _player = BScene.InstantiateMono(this, _library.playerViewPrefab, spawnPosition, quaternion.identity);
            }
            else
            {
                _player.body.position = spawnPosition;
            }
        }

        private void _move()
        {
            float deltaGameHours = _worldTime.GetHours() - _lastRecordedGameHours;

            _lastRecordedGameHours = _worldTime.GetHours();
            _currentPosition = BMath.MoveTowards(
                _currentPosition,
                _currentTarget,
                _playerConfig.moveSpeed * deltaGameHours
            );

            float2 worldPosition = _cameraTransform.WorldToScreen(_currentPosition);

            _player.body.position = new Vector3(worldPosition.x, worldPosition.y, 0);
            _gizmoHandle.DrawScreenDot(_currentTarget + new float2(-_halfTexelSize, _halfTexelSize));

            _gizmoHandle.DrawScreenDot(_currentTarget + new float2(-_halfTexelSize, -_halfTexelSize));
            _gizmoHandle.DrawScreenDot(_currentTarget + new float2(_halfTexelSize, -_halfTexelSize));
            _gizmoHandle.DrawScreenDot(_currentTarget + new float2(_halfTexelSize, _halfTexelSize));
            BDebug.Track(this, "target", _currentTarget);
            BDebug.Track(this, "coordinate", _currentPosition);
        }

        private void _onPressStarted(float2 position)
        {
            _startPressPosition = position;
        }

        private void _onPressCanceled(float2 position, float duration)
        {
            if (BMath.Distance(_startPressPosition, position) < 0.1f && duration < 0.2f)
            {
                float2 worldPosition = _cameraTransform.ScreenToWorld(position);

                float2 bottomLeft = BMath.Floor(worldPosition * WorldTransform.CellTileCount) /
                                    WorldTransform.CellTileCount;

                _currentTarget = bottomLeft + new float2(_halfTexelSize, _halfTexelSize);
                _cellIndex = (int2)BMath.Floor(worldPosition * WorldTransform.CellTileCount);

                BDebug.Track(this, "target cell index", _cellIndex);
                BDebug.Track(this, "test target cell index", (float2)_cellIndex / WorldTransform.CellTileCount);
            }
        }

        private float2 _indexToWorld(int2 index)
        {
            return (float2)index / WorldTransform.CellTileCount + new float2(_halfTexelSize, _halfTexelSize);
        }

        private UniTask<ActionResult> _dispose(ActionContext context)
        {
            _unsubscribeInput();

            return ActionResult.SuccessSyncronous();
        }

        private void _unsubscribeInput()
        {
            _input.GameInput.Click.PressStarted -= _onPressStarted;
            _input.GameInput.Click.PressCanceled -= _onPressCanceled;
        }
    }
}