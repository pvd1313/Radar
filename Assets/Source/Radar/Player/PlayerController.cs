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
    public class PlayerController : IGameCycleListener
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
        private float _halfTextelSize;


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
            handle.ListenOperation(ActionID.WARMUP, _warmup);
            handle.ListenUpdate(ActionID.TICK, _tick);
            handle.ListenOperation(ActionID.DISPOSE, _dispose);
            handle.ListenOperation(ActionID.GAME_LOAD, _gameLoad);
            handle.ListenOperation(ActionID.GAME_SAVE, _gameSave);
        }

        private UniTask<ActionResult> _warmup(ActionContext context)
        {
            _halfTextelSize = (1f / WorldTransform.CellTileCount) / 2.0f;
            
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
            
            BDebug.Track(this, "save game player", _game.player.position);
            
            return ActionResult.SuccessSyncronous();
        }

        private void _spawnPlayer()
        {
            _currentCoordinate = _indexToWorld(_game.player.position);
            _currentTarget = _indexToWorld(_game.player.position);
            float2 screenPosition = _cameraTransform.WorldToScreen(_currentCoordinate);
            float3 spawnPosition = new(screenPosition.x, screenPosition.y, 0);
         
            /*BDebug.LogInfo(this, $"spawn player ->" +
                                 $"| game position = {_game.player.position} " +
                                 $"| current target = {_currentTarget}" +
                                 $"| screen position = {screenPosition}" +
                                 $"| spawn position = {spawnPosition}");*/
            if (_player == null)
            {
                _player = BScene.InstantiateMono(this, _library.playerView, spawnPosition, quaternion.identity);
            }
            else
            {
                _player.body.position = spawnPosition;
            }
        }
        private void _move()
        {
            _currentCoordinate = BMath.MoveTowards(
                _currentCoordinate,
                _currentTarget,
                _playerConfig.moveSpeed);
 
            float2 worldPosition = _cameraTransform.WorldToScreen(_currentCoordinate);
 
            _player.body.position = new Vector3(worldPosition.x, worldPosition.y, 0);
            
            _gizmoHandle.DrawScreenDot(_currentTarget +  new float2(-_halfTextelSize,_halfTextelSize));
            _gizmoHandle.DrawScreenDot(_currentTarget + new float2(-_halfTextelSize,-_halfTextelSize));
            _gizmoHandle.DrawScreenDot(_currentTarget + new float2(_halfTextelSize,-_halfTextelSize));
            _gizmoHandle.DrawScreenDot(_currentTarget + new float2(_halfTextelSize,_halfTextelSize));
            
            BDebug.Track(this, "target", _currentTarget);
            BDebug.Track(this, "coordinate", _currentCoordinate);
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
                float2 worldPosition = _cameraTransform.ScreenToWorld(position);
 
                float2 bottomLeft = BMath.Floor(worldPosition * WorldTransform.CellTileCount) /
                                    WorldTransform.CellTileCount;
                
                _currentTarget = bottomLeft + new float2(_halfTextelSize, _halfTextelSize);
                _cellIndex = (int2)BMath.Floor(worldPosition * WorldTransform.CellTileCount);
                
                BDebug.Track(this, "target cell index", _cellIndex);
                BDebug.Track(this, "test target cell index", (float2)_cellIndex / WorldTransform.CellTileCount);
            }
        }
        
        private UniTask<ActionResult> _dispose(ActionContext context)
        {
            _unsubscribeInput();

            return ActionResult.SuccessSyncronous();
        }

        private float2 _indexToWorld(int2 index)
        {
            return (float2)index / WorldTransform.CellTileCount + new float2(_halfTextelSize, _halfTextelSize);
        }
    }
}