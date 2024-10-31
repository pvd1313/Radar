using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Gizmos;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Beast.Radar.Graphic;
using Beast.Radar.InputHandling;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Player
{
    public class PlayerController : Controller, IGameCycleListener
    {
        private readonly CorePrefabLibrary _library;
        private readonly PlayerConfig _playerConfig;
        private readonly GameModel _game;
        private readonly ICameraTransform _cameraTransform;
        private readonly IInputSystem _input;
        private readonly IGizmosManager _gizmosManager;
 
        private PlayerView _player;
 
        private float2 _currentTarget = new(512, 512);
        private float2 _currentCoordinate = new(512, 512);
        private int2 _cellIndex;
 
 
        private float2 _startPressPosition;
        private int _texelSize;
        private GizmosHandle _gizmoHandle;
        private float _halfTexelSize;


        public PlayerController(
            ICameraTransform cameraTransform,
            CorePrefabLibrary library,
            PlayerConfig playerConfig,
            IInputSystem input,
            IGizmosManager gizmosManager,
            GameModel game)
        {
            _cameraTransform = cameraTransform;
            _library = library;
            _playerConfig = playerConfig;
            _input = input;
            _gizmosManager = gizmosManager;
            _game = game;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
            handle.ListenBlocking(ActionID.TICK, _tick);
            handle.ListenBlocking(ActionID.DISPOSE, _dispose);
            handle.ListenConcurrent(ActionID.GAME_LOAD, _gameLoad);
            handle.ListenBlocking(ActionID.GAME_SAVE, _gameSave);
        }

        private UniTask _warmup(ConcurrentContext context)
        {
            _halfTexelSize = (1f / WorldTransform.CellTileCount) / 2.0f;
            
            _gizmoHandle = _gizmosManager.GetHandle(this, "cell points", Color.red);
            
            _subscribeInput();

            return UniTask.CompletedTask;
        }

        private void _tick(BlockingContext context)
        {
            // _move();
        }

        private UniTask _gameLoad(ConcurrentContext context)
        {
            // _spawnPlayer();
            
            return UniTask.CompletedTask;
        }

        private void _gameSave(BlockingContext context)
        {
            _game.player.position = _cellIndex;
            
            BDebug.Track(this, "save game player", _game.player.position);
        }

        // private void _spawnPlayer()
        // {
        //     _currentCoordinate = _indexToWorld(_game.player.position);
        //     _currentTarget = _indexToWorld(_game.player.position);
        //     float2 screenPosition = _cameraTransform.WorldToScreen(_currentCoordinate);
        //     float3 spawnPosition = new(screenPosition.x, screenPosition.y, 0);
        //  
        //     /*BDebug.LogInfo(this, $"spawn player ->" +
        //                          $"| game position = {_game.player.position} " +
        //                          $"| current target = {_currentTarget}" +
        //                          $"| screen position = {screenPosition}" +
        //                          $"| spawn position = {spawnPosition}");*/
        //     if (_player == null)
        //     {
        //         _player = BScene.InstantiateMono(this, _library.playerView, spawnPosition, quaternion.identity);
        //     }
        //     else
        //     {
        //         _player.body.position = spawnPosition;
        //     }
        // }
        // private void _move()
        // {
        //     _currentCoordinate = BMath.MoveTowards(
        //         _currentCoordinate,
        //         _currentTarget,
        //         _playerConfig.moveSpeed);
        //
        //     float2 worldPosition = _cameraTransform.WorldToScreen(_currentCoordinate);
        //
        //     _player.body.position = new Vector3(worldPosition.x, worldPosition.y, 0);
        //     
        //     _gizmoHandle.DrawScreenDot(_currentTarget +  new float2(-_halfTextelSize,_halfTextelSize));
        //     _gizmoHandle.DrawScreenDot(_currentTarget + new float2(-_halfTextelSize,-_halfTextelSize));
        //     _gizmoHandle.DrawScreenDot(_currentTarget + new float2(_halfTextelSize,-_halfTextelSize));
        //     _gizmoHandle.DrawScreenDot(_currentTarget + new float2(_halfTextelSize,_halfTextelSize));
        //     
        //     BDebug.Track(this, "target", _currentTarget);
        //     BDebug.Track(this, "coordinate", _currentCoordinate);
        // }
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
                float2 worldPosition = _cameraTransform.ScreenToWorld(position);
 
                float2 bottomLeft = BMath.Floor(worldPosition * WorldTransform.CellTileCount) /
                                    WorldTransform.CellTileCount;
                
                _currentTarget = bottomLeft + new float2(_halfTexelSize, _halfTexelSize);
                _cellIndex = (int2)BMath.Floor(worldPosition * WorldTransform.CellTileCount);
                
                BDebug.Track(this, "target cell index", _cellIndex);
                BDebug.Track(this, "test target cell index", (float2)_cellIndex / WorldTransform.CellTileCount);
            }
        }
        
        private void _dispose(BlockingContext context)
        {
            _unsubscribeInput();
        }

        private float2 _indexToWorld(int2 index)
        {
            return (float2)index / WorldTransform.CellTileCount + new float2(_halfTexelSize, _halfTexelSize);
        }
    }
}