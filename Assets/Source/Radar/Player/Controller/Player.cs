using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Claw.Gizmo;
using Beast.Claw.Graphic;
using Beast.Claw.Input;
using Beast.Claw.Math;
using Beast.Radar.GameTime;
using Beast.Radar.Graphic;
using Beast.Radar.Serialization;
using Beast.Radar.Terrain;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

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
        private readonly PlayerPreference _prefs;
        private readonly IGizmoManager _gizmoManager;
        private readonly IInputManager _inputManager;
        
        private bool _isMoving;
        private bool _hasTarget;
        private float2 _position;
        private float2 _target;
        private PlayerNavigator _navigator;
        
        public Player(
            IGameTime gameTime,
            PlayerConfig config,
            ICameraTransform camera,
            ITerrainBackedBiome biome,
            GameModel game,
            PlayerPreference prefs,
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

        public float GetRemainingDistanceToTarget()
        {
            if (!_hasTarget)
            {
                return 0;
            }

            return _navigator.GetRemainingDistance();
        }

        protected override UniTask _warmup(ConcurrentContext arg)
        {
            _inputManager.RegisterConsumer(this, InputConsumerPriority.WORLD_MAP_NAVIGATION);

            _navigator = new PlayerNavigator(_gizmoManager, _config, _biome);
            _navigator.Warmup();
            
            return UniTask.CompletedTask;
        }

        protected override UniTask _gameLoad(ConcurrentContext arg)
        {
            _position = _game.player.position;
            _isMoving = false;
            _hasTarget = false;
            
            IsReady = true;

            return UniTask.CompletedTask;
        }

        protected override void _gameSave(BlockingContext obj)
        {
            _game.player.position = BWorld.WorldToTileIndex(_position);
        }

        protected override void _tick(BlockingContext obj)
        {
            if (!_isMoving || !_hasTarget)
            {
                return;
            }
            
            float maxDelta = (float)_gameTime.Delta.TotalHours * _config.movementSpeedKilometresPerHour;
            
            _navigator.MovePlayer(ref _position, maxDelta, out bool isDestinationReached);

            if (isDestinationReached)
            {
                _hasTarget = false;
                _isMoving = false;
            }
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
                _setTargetSetupPath(targetPosition);

                if (_prefs.beginMovementOnSingleTap && _hasTarget)
                {
                    _isMoving = true;
                }
            }

            if (tap.tapCount == 2 && _hasTarget && !_prefs.beginMovementOnSingleTap)
            {
                _isMoving = true;
            }

            if (Build.IS_DEVELOPMENT)
            {
                if (tap.tapCount == 3)
                {
                    _isMoving = false;
                    _hasTarget = false;

                    float2 tapWorldPosition = _camera.ViewportToWorld(tap.position);
                    _position = BWorld.SnapToTileGridCenter(tapWorldPosition);
                    
                    _navigator.Clear();
                }
            }
        }

        private void _setTargetSetupPath(float2 target)
        {
            float textureSize = _biome.Terrain1kmCell.blockSize * _biome.Terrain1kmCell.blockCountX - 1;

            target.x = BMath.Clamp(target.x, 0, textureSize - 1);
            target.y = BMath.Clamp(target.y, 0, textureSize - 1);
            
            _target = BWorld.SnapToTileGridCenter(target);
            
            _hasTarget = true;
            
            _navigator.Setup(_position, _target);
        }

        protected override void _dispose(BlockingContext obj)
        {
            _navigator?.Dispose();
        }
    }
}