using System;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Gizmos;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Beast.Radar.Graphic;
using Beast.Radar.InputHandling;
using Beast.Radar.Serialization;
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
        private readonly IWorldTime _time;
        private readonly PlayerConfig _config;
        private readonly ICameraTransform _camera;
        private readonly GameModel _gameModel;
        private readonly PlayerPreferences _preferences;
        private readonly IInputSystem _input;

        private InputValueControl<float2> _clickInput;
        private float _halfTexelSize;
        private float2 _startPressPosition;
        private float _startPressTime;
        private float _lastRecordedHours;
        private bool _isCheckClick;

        public PlayerController(
            IWorldTime time,
            IInputSystem input,
            PlayerConfig config,
            ICameraTransform camera,
            GameModel gameModel,
            PlayerPreferences preferences)
        {
            _time = time;
            _config = config;
            _camera = camera;
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

            IsReady = true;

            return UniTask.CompletedTask;
        }

        private void _gameSave(BlockingContext obj)
        {
            _gameModel.player.position = _worldToIndex(Position);
        }
        
        private void _tick(BlockingContext obj)
        {
            _followClick();

            if (IsMovement)
            {
                if (Position.Equals(Target))
                {
                    SwitchMovement();
                    return;
                }
                
                float deltaGameHours = _time.GetTotalHours() - _lastRecordedHours;

                TempPosition = BMath.MoveTowards(TempPosition, Target, Speed * deltaGameHours);
                int2 cell = _worldToIndex(TempPosition);
                Position = _indexToWorld(cell);
                

                BDebug.Track(this, "temp position", TempPosition);
                BDebug.Track(this, "position", Position);
            }

            _lastRecordedHours = _time.GetTotalHours();
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
                        if (_clickInput.TapCount == 1 )
                        {
                            _setTarget(_camera.ScreenToWorld(_startPressPosition));
                            if (IsMovement)
                            {
                                SwitchMovement();
                            }
                        }
                        else if(_clickTimer < MULTI_CLICK_DELAY && _clickInput.TapCount == 2 && !IsMovement)
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
            BDebug.LogInfo(this, $"{worldTarget}->{index}->{Target}");
            ChangeTarget?.Invoke();
        }
        
        private float2 _indexToWorld(int2 index)
        {
            return (float2)index / WorldTransform.CellTileCount + new float2(_halfTexelSize, _halfTexelSize);
        }

        private int2 _worldToIndex(float2 worldPosition)
        {
            return (int2)BMath.Floor(worldPosition * WorldTransform.CellTileCount);
        }

        public Color GizmosColor { get; } = Color.magenta;
        public void Draw(GizmosHandle handle)
        {
            if (!IsReady)
            {
                return;
            }
            handle.DrawScreenDot(Position + new float2(_halfTexelSize,_halfTexelSize));
            handle.DrawScreenDot(Position + new float2(_halfTexelSize,-_halfTexelSize));
            handle.DrawScreenDot(Position + new float2(-_halfTexelSize,-_halfTexelSize));
            handle.DrawScreenDot(Position + new float2(-_halfTexelSize,-_halfTexelSize));
        }
    }
}