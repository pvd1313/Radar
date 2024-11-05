using System;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Gizmos;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Beast.Radar.GameTime;
using Beast.Radar.Graphic;
using Beast.Radar.InputHandling;
using Beast.Radar.Player.Path;
using Beast.Radar.Serialization;
using Beast.Radar.Terrain;
using Beast.Radar.World;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Player
{
    public class PlayerController : Controller, IPlayerController, IGameCycleListener, IGizmosDrawer
    {
        public event Action ChangeMovementState;
        public event Action ChangeTarget;
        public bool IsReady { get; private set; }
        public float2 TempPosition { get; private set; }
        public float2 Position { get; private set; }
        public float2 Target { get; private set; }
        public float Speed { get; private set; }
        public bool IsMovement { get; private set; }

        private const float MULTI_CLICK_DELAY = 0.3f;

        private float _clickTimer;
        private readonly IGameTime _time;
        private readonly PlayerConfig _config;
        private readonly ICameraTransform _camera;
        private readonly ITerrainBackedBiome _biome;
        private readonly GameModel _gameModel;
        private readonly PlayerPreferences _preferences;
        private readonly IInputSystem _input;

        private Pathfinding _pathfinding;
        private InputValueControl<float2> _clickInput;
        private float _halfTexelSize;
        private float2 _startPressPosition;
        private float _startPressTime;
        private float _lastRecordedHours;
        private bool _isCheckClick;
        private int2[] _path;

        public PlayerController(
            IGameTime time,
            IInputSystem input,
            PlayerConfig config,
            ICameraTransform camera,
            ITerrainBackedBiome biome,
            GameModel gameModel,
            PlayerPreferences preferences)
        {
            _time = time;
            _config = config;
            _camera = camera;
            _biome = biome;
            _gameModel = gameModel;
            _preferences = preferences;
            _input = input;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
            handle.ListenConcurrent(ActionID.GAME_LOAD, _gameLoad);
            handle.ListenBlocking(ActionID.TICK, _tick);
            handle.ListenBlocking(ActionID.GAME_SAVE, _gameSave);
            handle.ListenBlocking(ActionID.DISPOSE, _dispose);
        }

        public void SwitchMovement()
        {
            IsMovement = !IsMovement;
            ChangeMovementState?.Invoke();
        }

        private UniTask _warmup(ConcurrentContext arg)
        {
            _clickInput = _input.GameInput.Click;

            _halfTexelSize = 1f / WorldTransform.CellTileCount / 2.0f;

            Speed = _config.moveSpeed;

            return UniTask.CompletedTask;
        }

        private UniTask _gameLoad(ConcurrentContext arg)
        {
            Position = _indexToWorld(_gameModel.player.position);

            TempPosition = Position;

            Target = Position;

            int size = _biome.Terrain8kmCell.cellCountX;
            _pathfinding = new Pathfinding(size,
                _config.GetTileSpeedModifiers(),
                _biome.Terrain8kmCell.texture);

            BDebug.LogInfo(this, $"Create pathfinding {size}");

            IsReady = true;

            return UniTask.CompletedTask;
        }

        private void _gameSave(BlockingContext obj)
        {
            _gameModel.player.position = _worldToIndex(Position);
        }

        private int _currentPathCell;

        private void _tick(BlockingContext obj)
        {
            if (_biome.Terrain8kmCell.texture.Length > 0)
            {
                float2 mouse = _input.GameInput.ScreenPoint.Value;
                float2 mouseWorld = _camera.ScreenToWorld(mouse);
                BDebug.Track(this, "8km index", _worldToBlock(mouseWorld));
            }

            // if (_pathfinding == null && _backedBiome.Terrain8kmCell.texture.Length > 0)
            // {
            //     int size = _backedBiome.Terrain8kmCell.cellCountX;
            //     _pathfinding = new Pathfinding(size,
            //         _config.GetTileSpeedModifiers(),
            //         _backedBiome.Terrain8kmCell.texture);
            //  
            //     //BDebug.LogInfo(this, $"Create pathfinding {size}");
            // }

            _followClick();

            /*if (IsMovement)
            {
                if (Position.Equals(Target))
                {
                    SwitchMovement();
                    return;
                }

                /*float deltaGameHours = _time.GetTotalHours() - _lastRecordedHours;

               TempPosition = BMath.MoveTowards(TempPosition, _path[_currentPathCell], Speed * deltaGameHours);
               int2 cell = _worldToIndex(TempPosition);
               Position = _indexToWorld(cell);

               if (TempPosition.Equals(_path[_currentPathCell]))
               {
                   _currentPathCell++;
               }#1#

                BDebug.Track(this, "temp position", TempPosition);
                BDebug.Track(this, "position", Position);
            }

            _lastRecordedHours = _time.GetTotalHours();*/
        }

        private void _followClick()
        {
            if (_clickTimer < MULTI_CLICK_DELAY)
            {
                _clickTimer += Time.deltaTime;
            }

            if (_clickInput.State == EButtonState.Pressed && !_isCheckClick)
            {
                _startPressPosition = _clickInput.Value;
                _isCheckClick = true;
            }

            if (_isCheckClick && _clickInput.State == EButtonState.Default)
            {
                if (BMath.Distance(_startPressPosition, _clickInput.LastValue) < 0.1f)
                {
                    if (!_preferences.isSingleTapMovement)
                    {
                        if (_clickInput.TapCount == 1)
                        {
                            _setTarget(_camera.ScreenToWorld(_startPressPosition));

                            if (IsMovement)
                            {
                                SwitchMovement();
                            }
                        }
                        else if (_clickTimer < MULTI_CLICK_DELAY && _clickInput.TapCount == 2 && !IsMovement)
                        {
                            SwitchMovement();
                        }
                    }
                    else if (_clickInput.TapCount == 1)
                    {
                        _setTarget(_camera.ScreenToWorld(_startPressPosition));

                        if (!IsMovement)
                        {
                            SwitchMovement();
                        }
                    }

                    BDebug.Track(this, "target", Target);
                    BDebug.Track(this, "target cell", _worldToIndex(Target));
                }

                _clickTimer = 0;
                _isCheckClick = false;
            }
        }

        private void _setTarget(float2 worldTarget)
        {
            int2 index = _worldToIndex(worldTarget);
            Target = _indexToWorld(index);

            BDebug.Track(this, $"8km target", _worldToBlock(Target));
            BDebug.Track(this, $"8km position", _worldToBlock(Position));

            _path = _pathfinding.FindPathArray(
                _worldToBlock(Position),
                _worldToBlock(Target),
                out long time);

            //BDebug.LogInfo(this, $"Find path: lenght = {_path.Length} time = {time}ms");

            /*
            _currentPathCell = 0;
            ChangeTarget?.Invoke();*/
        }


        private int2 _blockToWorld(int2 block)
        {
            return _biome.Terrain8kmCell.CellToWorldPoint(block);
        }

        private int2 _worldToBlock(float2 worldPosition)
        {
            return _biome.Terrain8kmCell.WorldPositionToCell(worldPosition);
        }

        private float2 _indexToWorld(int2 index)
        {
            return (float2)index / WorldTransform.CellTileCount + new float2(_halfTexelSize, _halfTexelSize);
        }

        private int2 _worldToIndex(float2 worldPosition)
        {
            return (int2)BMath.Floor(worldPosition * WorldTransform.CellTileCount);
        }

        private void _dispose(BlockingContext obj)
        {
            _pathfinding?.Dispose();
        }

        public Color GizmosColor { get; } = Color.magenta;

        public void Draw(GizmosHandle handle)
        {
            if (!IsReady)
            {
                return;
            }

            if (_path != null && _path.Length > 0)
            {
                foreach (int2 point in _path)
                {
                    handle.DrawScreenDot(_blockToWorld(point));
                }
            }

            handle.DrawScreenDot(Position + new float2(_halfTexelSize, _halfTexelSize));
            handle.DrawScreenDot(Position + new float2(_halfTexelSize, -_halfTexelSize));
            handle.DrawScreenDot(Position + new float2(-_halfTexelSize, -_halfTexelSize));
            handle.DrawScreenDot(Position + new float2(-_halfTexelSize, -_halfTexelSize));
        }
    }
}