using Beast.Claw.Debug;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Beast.Radar.Graphic;
using Beast.Radar.InputHandling;
using Beast.Radar.World;
using Unity.Mathematics;

namespace Beast.Radar.Player
{
    public class PlayerMovement : IPlayerMovement
    {
        private readonly PlayerView _view;
        private readonly IWorldTime _time;
        private readonly IInputSystem _input;
        private readonly PlayerConfig _config;
        private readonly ICameraTransform _camera;

        private float2 _currentTarget;
        private float2 _currentPosition;
        private int2 _targetCell;

        private float2 _startPressPosition;
        private float _halfTexelSize;
        private float _lastRecordedHours;
        
        public PlayerMovement(
            PlayerView view, 
            IWorldTime time, 
            IInputSystem input, 
            PlayerConfig config,
            ICameraTransform camera)
        {
            _view = view;
            _time = time;
            _input = input;
            _config = config;
            _camera = camera;
        }

        public void Warmup()
        {
            _halfTexelSize = (1f / WorldTransform.CellTileCount) / 2.0f;
            _subscribeInput();
        }
        
        public void Dispose()
        {
            _unsubscribeInput();
        }

        public int2 GetPosition()
        {
            return _worldToIndex(_currentPosition);
        }

        public void SetPosition(int2 cellIndex)
        {
            _view.body.position = _indexToWorld(cellIndex).AsVector3();
        }

        public void Move()
        {
            float deltaGameHours = _time.GetTotalHours() - _lastRecordedHours;
            _lastRecordedHours = _time.GetTotalHours();

            _currentPosition = BMath.MoveTowards(
                _currentPosition,
                _currentTarget,
                _config.moveSpeed * deltaGameHours
            );

            _view.body.position = _camera.WorldToScreen(_currentPosition).AsVector3();
            _view.target.position = _camera.WorldToScreen(_currentTarget).AsVector3();

            if (_view.target.gameObject.activeSelf != BMath.Distance(_currentPosition, _currentTarget) > 0.1f)
            {
                _view.target.gameObject.SetActive(!_view.target.gameObject.activeSelf);
            }
            
            BDebug.Track(this, "position", _currentPosition);
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
                _currentTarget = _camera.ScreenToWorld(position);
                _targetCell = _worldToIndex(_currentTarget);

                if (_currentTarget.y > _currentPosition.y)
                {
                    _view.target.SetAsFirstSibling();
                }
                else
                {
                    _view.target.SetAsLastSibling();
                }
                
                BDebug.Track(this, "target", _currentTarget);
                BDebug.Track(this, "target cell", _targetCell);
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