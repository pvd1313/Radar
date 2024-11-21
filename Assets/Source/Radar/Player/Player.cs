using System;
using System.Diagnostics;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Claw.Gizmo;
using Beast.Claw.Graphic;
using Beast.Claw.Input;
using Beast.Claw.Jobs;
using Beast.Claw.Math;
using Beast.Radar.GameTime;
using Beast.Radar.Graphic;
using Beast.Radar.Navigation;
using Beast.Radar.Serialization;
using Beast.Radar.Terrain;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Player
{
    public class Player : Actor, IPlayer, IInputConsumer
    {
        public Int32Mask SupportedAxis { get; } = AxisType.TAP.AsMask();
        
        public bool IsReady { get; private set; }

        public float2 Position => _position;
        public float2 Target => _target;
        public bool IsMoving => _isMoving;
        public bool HasTarget => _hasTarget;

        private readonly IGameTime _gameTime;
        private readonly PlayerConfig _config;
        private readonly ICameraTransform _camera;
        private readonly ITerrainBackedBiome _biome;
        private readonly GameModel _game;
        private readonly PlayerPreferences _prefs;
        private readonly IGizmoManager _gizmoManager;
        private readonly IInputManager _inputManager;
        
        private bool _isMoving;
        private bool _hasTarget;
        private float2 _position;
        private float2 _target;
        
        private float _halfTexelSize;
        private float _lastRecordedHours;
        private NativeList<int2> _path;
        private Stopwatch _pathWatch;
        private IGizmoHandle _pathGizmo;
        private PathBuffer _pathBuffer;
        private int _currentPathCell;

        public Player(
            IGameTime gameTime,
            PlayerConfig config,
            ICameraTransform camera,
            ITerrainBackedBiome biome,
            GameModel game,
            PlayerPreferences prefs,
            IGizmoManager gizmoManager,
            IInputManager inputManager)
        {
            _gameTime = gameTime;
            _config = config;
            _camera = camera;
            _biome = biome;
            _game = game;
            _prefs = prefs;
            _gizmoManager = gizmoManager;
            _inputManager = inputManager;
        }

        public void ToggleMovementState()
        {
            _isMoving = !_isMoving;
        }

        protected override UniTask _warmup(ConcurrentContext arg)
        {
            _halfTexelSize = 1f / WorldTransform.CellTileCount / 2.0f;

            if (Build.IS_DEVELOPMENT)
            {
                // TODO: #R-93 Restore.
                // _input.DebugInput.TeleportButton.PressStarted += () =>
                // {
                //     Position = _camera.ScreenToWorld(_input.GameInput.ScreenPoint.Value);
                //     Target = Position;
                // };
            }
            
            _inputManager.RegisterConsumer(this, InputConsumerPriority.WORLD_MAP_NAVIGATION);

            _path = BNative.CreatePermList<int2>(256);
            _pathWatch = new Stopwatch();
            _pathGizmo = _gizmoManager.CreateHandle(this);

            _pathBuffer = new PathBuffer
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
            _position = _game.player.position;
            _target = _position;
            _isMoving = false;
            
            IsReady = true;

            return UniTask.CompletedTask;
        }

        protected override void _gameSave(BlockingContext obj)
        {
            _game.player.position = _worldToIndex(Position);
        }

        protected override void _tick(BlockingContext obj)
        {
            if (!_isMoving)
            {
                return;
            }

            float distance = BMath.Distance(_position, _target);
            float finishDistance = 1f / WorldTransform.CellTileCount * 0.5f;

            if (distance < finishDistance)
            {
                ToggleMovementState();

                _hasTarget = false;
                
                return;
            }

            float maxDelta = (float)_gameTime.Delta.TotalHours * _config.movementSpeedKilometresPerHour;
            
            _position = BMath.MoveTowards(_position, _target, maxDelta);

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
        
        public bool TryCaptureAxis(PackedAxis axis)
        {
            return _camera.IsReady;
        }

        public void UpdateAxis(PackedAxis axis)
        {
            axis.Unpack(out TapAxis tap);

            float2 targetPosition = _camera.ViewportToWorld(tap.position);

            if (tap.tapCount == 1)
            {
                _setTargetAndBuildPath(targetPosition);

                if (_prefs.beginMovementOnSingleTap && _hasTarget)
                {
                    _isMoving = true;
                }
            }

            if (tap.tapCount == 2 && _hasTarget && !_prefs.beginMovementOnSingleTap)
            {
                _isMoving = true;
            }
        }

        private void _setTargetAndBuildPath(float2 worldTarget)
        {
            int2 index = _worldToIndex(worldTarget);

            _target = _indexToWorld(index);
            
            FindPathJob job = new ()
            {
                startPosition = _worldToBlock(Position),
                endPosition = _worldToBlock(Target),
                tileCosts = _config.TileSpeedModifiersToTileCost(),
                textureSizeX = _biome.Terrain8kmCell.cellCountX,
                texture = _biome.Terrain8kmCell.texture,
                outputPath = _path,
                buffer = _pathBuffer
            };
            
            _pathWatch.Restart();
            
            job.Schedule().Complete();

            float elapsedMS = _pathWatch.ElapsedMillisecondsFloat();

            BDebug.LogInfo(this, $"Found path (nodeCount = {_path.Length}; time = {elapsedMS:N2}ms)");

            _drawPathGizmo();

            _hasTarget = true;

            /*
            _currentPathCell = 0;
            ChangeTarget?.Invoke();*/
        }

        [Conditional("DEVELOPMENT")]
        private void _drawPathGizmo()
        {
            _pathGizmo.Clear();
            _pathGizmo.SetColor(Color.magenta.WithAlpha(0.4f));
            
            float cellSize = _biome.Terrain8kmCell.cellSize;
            float2 cellSizeHalf = cellSize * 0.5f;
            
            if (_path.Length > 0)
            {
                foreach (int2 point in _path)
                {
                    float2 position = _blockToWorld(point) + cellSizeHalf;

                    _pathGizmo.AddQuad(position, cellSize);
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
            
            _pathBuffer.Dispose();
            _pathGizmo.Dispose();
        }
    }
}