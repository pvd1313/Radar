using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Beast.Radar.Graphic;
using Beast.Radar.InputHandling;
using Beast.Radar.Serialization;
using Beast.Radar.World;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

namespace Beast.Radar.Player
{
    public class PlayerMovement : Controller, IPlayerMovement, IGameCycleListener
    {
        public bool IsReady { get; private set; }
        public float2 Position { get; private set; }
        public float2 Target { get; private set; }
        public float Speed { get; private set; }

        private readonly IPlayerState _state;
        private readonly IWorldTime _time;
        private readonly PlayerConfig _config;
        private readonly ICameraTransform _camera;
        private readonly GameModel _gameModel;
        private readonly IInputSystem _input;

        private InputValueControl<float2> _clickInput;
        private float _halfTexelSize;
        private float2 _startPressPosition;
        private float _startPressTime;
        private float _lastRecordedHours;
        private bool _isCheckClick;


        public PlayerMovement(
            IPlayerState state,
            IWorldTime time,
            IInputSystem input,
            PlayerConfig config,
            ICameraTransform camera,
            GameModel gameModel)
        {
            _state = state;
            _time = time;
            _config = config;
            _camera = camera;
            _gameModel = gameModel;
            _input = input;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
            handle.ListenConcurrent(ActionID.GAME_LOAD, _gameLoad);
            handle.ListenBlocking(ActionID.TICK, _tick);
            handle.ListenBlocking(ActionID.GAME_SAVE, _gameSave);
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
            
            Target = Position;
            
            IsReady = true;
            
            return UniTask.CompletedTask;
        }

        private void _tick(BlockingContext obj)
        {
            _followClick();

            float deltaGameHours = _time.GetTotalHours() - _lastRecordedHours;
         
            _lastRecordedHours = _time.GetTotalHours();

            if (_state.TryGetState(EPlayerState.IS_MOVE.ToString(), out bool isMove) && isMove)
            {
                //move toward
            }
            Position = BMath.MoveTowards(Position, Target, Speed * deltaGameHours);

            BDebug.Track(this, "position", Position);
        }

        private void _gameSave(BlockingContext obj)
        {
            _gameModel.player.position = _worldToIndex(Position);
        }

        private void _followClick()
        {
            if (_clickInput.State == EButtonState.Pressed && !_isCheckClick)
            {
                _startPressPosition = _clickInput.Value;
             
                _isCheckClick = true;
            }

            if (_isCheckClick && _clickInput.State == EButtonState.Default)
            {
                if (_clickInput.PressedTime < 0.2f && BMath.Distance(_startPressPosition, _clickInput.LastValue) < 0.1f)
                {
                    Target = _camera.ScreenToWorld(_startPressPosition);
                    
                    BDebug.Track(this, "target", Target);
                    BDebug.Track(this, "target cell", _worldToIndex(Target));
                }

                _isCheckClick = false;
            }
        }

        private float2 _indexToWorld(int2 index)
        {
            return (float2)index / WorldTransform.CellTileCount + new float2(_halfTexelSize, _halfTexelSize);
        }

        private int2 _worldToIndex(float2 worldPosition)
        {
            return (int2)BMath.Floor(worldPosition * WorldTransform.CellTileCount);
        }
    }
}