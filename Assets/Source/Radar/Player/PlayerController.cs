using System;
using System.Diagnostics;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Claw.Gizmo;
using Beast.Claw.Graphic;
using Beast.Claw.Jobs;
using Beast.Claw.Math;
using Beast.Radar.GameTime;
using Beast.Radar.Graphic;
using Beast.Radar.Player.Path;
using Beast.Radar.Serialization;
using Beast.Radar.Terrain;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Player
{
    public class PlayerController : Actor, IPlayerController
    {
        public event Action ChangeMovementState;
        public event Action ChangeTarget;
        
        public bool IsReady { get; private set; }
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
        private readonly IGizmoManager _gizmoManager;

        private float _halfTexelSize;
        private float2 _startPressPosition;
        private float _startPressTime;
        private float _lastRecordedHours;
        private bool _isCheckClick;
        private NativeList<int2> _path;
        private Stopwatch _pathWatch;
        private PathBuffer _buffer;
        private IGizmoHandle _gizmo;

        public PlayerController(
            IGameTime time,
            PlayerConfig config,
            ICameraTransform camera,
            ITerrainBackedBiome biome,
            GameModel gameModel,
            PlayerPreferences preferences,
            IGizmoManager gizmoManager)
        {
            _time = time;
            _config = config;
            _camera = camera;
            _biome = biome;
            _gameModel = gameModel;
            _preferences = preferences;
            _gizmoManager = gizmoManager;
        }

        public void SwitchMovement()
        {
            IsMovement = !IsMovement;
            ChangeMovementState?.Invoke();
        }

        protected override UniTask _warmup(ConcurrentContext arg)
        {
            _halfTexelSize = 1f / WorldTransform.CellTileCount / 2.0f;

            Speed = _config.moveSpeed;

            if (Build.IS_DEVELOPMENT)
            {
                // TODO: #R-93 Restore.
                // _input.DebugInput.TeleportButton.PressStarted += () =>
                // {
                //     Position = _camera.ScreenToWorld(_input.GameInput.ScreenPoint.Value);
                //     Target = Position;
                // };
            }

            _path = Native.CreatePermList<int2>(256);
            _pathWatch = new Stopwatch();
            _gizmo = _gizmoManager.CreateHandle(this);

            _buffer = new PathBuffer
            {
                openList = new PriorityQueue(Allocator.Persistent),
                outputPath = new NativeList<int2>(Allocator.Persistent),
                pathNodeList = new NativeList<PathNode>(_config.maxNodeCount, Allocator.Persistent),
                positionToIndexMap = new NativeHashMap<int2, int>(_config.maxNodeCount, Allocator.Persistent),
                closedList = new NativeHashSet<int>(_config.maxNodeCount, Allocator.Persistent),
                openSet = new NativeHashSet<int>(_config.maxNodeCount, Allocator.Persistent),
            };

            return UniTask.CompletedTask;
        }

        protected override UniTask _gameLoad(ConcurrentContext arg)
        {
            Position = _gameModel.player.position;

            Target = Position;
            
            IsReady = true;

            BDebug.Track(this, "position", Position);
            BDebug.Track(this, "target", Target);
            BDebug.Track(this, "target cell", _worldToIndex(Target));

            return UniTask.CompletedTask;
        }

        protected override void _gameSave(BlockingContext obj)
        {
            _gameModel.player.position = _worldToIndex(Position);
        }

        private int _currentPathCell;

        protected override void _tick(BlockingContext obj)
        {
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

            // TODO: #R-93 Restore.
            // if (_clickInput.State == EButtonState.Pressed && !_isCheckClick)
            // {
            //     _startPressPosition = _clickInput.Value;
            //     _isCheckClick = true;
            // }
            //
            // if (_isCheckClick && _clickInput.State == EButtonState.Default)
            // {
            //     if (BMath.Distance(_startPressPosition, _clickInput.LastValue) < 0.1f)
            //     {
            //         if (!_preferences.isSingleTapMovement)
            //         {
            //             if (_clickInput.TapCount == 1)
            //             {
            //                 _setTarget(_camera.ScreenToWorld(_startPressPosition));
            //
            //                 if (IsMovement)
            //                 {
            //                     SwitchMovement();
            //                 }
            //             }
            //             else if (_clickTimer < MULTI_CLICK_DELAY && _clickInput.TapCount == 2 && !IsMovement)
            //             {
            //                 SwitchMovement();
            //             }
            //         }
            //         else if (_clickInput.TapCount == 1)
            //         {
            //             _setTarget(_camera.ScreenToWorld(_startPressPosition));
            //
            //             if (!IsMovement)
            //             {
            //                 SwitchMovement();
            //             }
            //         }
            //
            //         BDebug.Track(this, "position", Position);
            //         BDebug.Track(this, "target", Target);
            //         BDebug.Track(this, "target cell", _worldToIndex(Target));
            //     }

            //     _clickTimer = 0;
            //     _isCheckClick = false;
            // }
        }

        private void _setTarget(float2 worldTarget)
        {
            int2 index = _worldToIndex(worldTarget);
            
            Target = _indexToWorld(index);

            FindPathJob job = new ()
            {
                startPosition = _worldToBlock(Position),
                endPosition = _worldToBlock(Target),
                tileCosts = _config.TileSpeedModifiersToTileCost(),
                textureSizeX = _biome.Terrain8kmCell.cellCountX,
                texture = _biome.Terrain8kmCell.texture,
                outputPath = _path,
                buffer = _buffer
            };
            
            _pathWatch.Restart();
            
            job.Schedule().Complete();

            float elapsedMS = _pathWatch.ElapsedMillisecondsFloat();

            BDebug.LogInfo(this, $"Found path (nodeCount = {_path.Length}; time = {elapsedMS:N2}ms)");

            _drawPathGizmo();

            /*
            _currentPathCell = 0;
            ChangeTarget?.Invoke();*/
        }

        [Conditional("DEVELOPMENT")]
        private void _drawPathGizmo()
        {
            _gizmo.Clear();
            _gizmo.SetColor(Color.magenta.WithAlpha(0.4f));
            
            float cellSize = _biome.Terrain8kmCell.cellSize;
            float2 cellSizeHalf = cellSize * 0.5f;
            
            if (_path.Length > 0)
            {
                foreach (int2 point in _path)
                {
                    float2 position = _blockToWorld(point) + cellSizeHalf;

                    _gizmo.AddQuad(position, cellSize);
                }
            }
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

        protected override void _dispose(BlockingContext obj)
        {
            if (_path.IsCreated)
            {
                _path.Dispose();
            }
            
            _buffer.Dispose();
            _gizmo.Dispose();
        }
    }
}