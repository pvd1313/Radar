using Beast.Claw.Debug;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Beast.Radar.Graphic;
using Beast.Radar.InputHandling;
using Beast.Radar.World;
using Unity.Mathematics;

namespace Beast.Radar.Player
{
    public class PlayerPosition : IPlayerPosition
    {
        private readonly PlayerView _view;
        private readonly IWorldTime _time;
        private readonly PlayerConfig _config;
        private readonly ICameraTransform _camera;
        private readonly InputValueControl<float2> _clickInput;
        private readonly float _halfTexelSize;

        private float2 _currentTarget;
        private float2 _currentPosition;
        private int2 _targetCell;

        private float2 _startPressPosition;
        private float _startPressTime;
        private float _lastRecordedHours;
        private bool _isCheckClick;

        public PlayerPosition(
            PlayerView view,
            IWorldTime time,
            IInputSystem input,
            PlayerConfig config,
            ICameraTransform camera) 
        {
            _view = view;
            _time = time;
            _config = config;
            _camera = camera;
            
            _clickInput = input.GameInput.Click;
            _halfTexelSize = (1f / WorldTransform.CellTileCount) / 2.0f;
        }

        public int2 GetPosition()
        {
            return _worldToIndex(_currentPosition);
        }

        public void SetPosition(int2 cellIndex)
        {
            _view.body.position = _indexToWorld(cellIndex).XY0();
            _currentPosition = _view.body.position.XY();
        }

        public void Update()
        {
            _followClick();

            float deltaGameHours = _time.GetTotalHours() - _lastRecordedHours;
            _lastRecordedHours = _time.GetTotalHours();

            _currentPosition = BMath.MoveTowards(
                _currentPosition,
                _currentTarget,
                _config.moveSpeed * deltaGameHours
            );

            _view.body.position = _camera.WorldToScreen(_currentPosition).XY0();
            _view.target.position = _camera.WorldToScreen(_currentTarget).XY0();

            if (_view.target.gameObject.activeSelf != BMath.Distance(_currentPosition, _currentTarget) > 0.1f)
            {
                _view.target.gameObject.SetActive(!_view.target.gameObject.activeSelf);
            }
            
            float distance = BMath.Distance(_currentPosition, _currentTarget);
            string travelTime = _time.CalculateTravelTime(_currentPosition, _currentTarget, _config.moveSpeed);
            _view.targetText.SetText($"{distance:F0}\n{travelTime}");

            BDebug.Track(this, "position", _currentPosition);
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
                if (_clickInput.PressedTime < 0.2f && BMath.Distance(_clickInput.LastValue, _startPressPosition) < 0.1f)
                {
                    _setTarget(_startPressPosition);
                }
                
                _isCheckClick = false;
            }
        }

        private void _setTarget(float2 position)
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