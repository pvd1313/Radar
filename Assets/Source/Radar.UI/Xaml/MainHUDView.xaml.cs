using Beast.Claw.UI;
using Noesis;

namespace Beast.Radar.UI
{
    public class MainHUDView : BeastControl
    {
        public TextBlock textTime;
        public Button buttonSettings;
        public Button autoPathFindingButton;
        public Button buttonMove;
        
        private Brush _brushActiveColor;
        private Brush _brushDisableColor;
        
        private Brush _brushStandImage;
        private Brush _brushMoveImage;

        private bool _isPathFindingActive;
        private int _playerMovementCache;
        
        public override void BindElements()
        {
            _find("text_time", out textTime);
            _find("button_settings", out buttonSettings);
            _find("button_path", out autoPathFindingButton);
            _find("button_move", out buttonMove);
            
            _brushActiveColor = (SolidColorBrush)FindResource("Brush.Element.Light.Active");
            _brushDisableColor = (SolidColorBrush)FindResource("Brush.Element.Light.Disable");
            
            _brushMoveImage = (ImageBrush)FindResource("Brush.Image.Move");
            _brushStandImage = (ImageBrush)FindResource("Brush.Image.Stand");

            _isPathFindingActive = true;
            _playerMovementCache = -1;
            
            SetPlayerMovementState(false, false);
            SetAutoPathFindingState(false);
        }
        
        public void SetPlayerMovementState(bool hasTarget, bool isMoving)
        {
            int targetValue;
            
            if (!hasTarget)
            {
                targetValue = 0;
            }
            else
            {
                targetValue = isMoving ? 1 : 2;
            }

            if (targetValue != _playerMovementCache)
            {
                _playerMovementCache = targetValue;

                buttonMove.OpacityMask = isMoving ? _brushMoveImage : _brushStandImage;
                buttonMove.Visibility = hasTarget ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public void SetAutoPathFindingState(bool isActive)
        {
            if (_isPathFindingActive == isActive)
            {
                return;
            }

            _isPathFindingActive = isActive;
            
            autoPathFindingButton.Background = _isPathFindingActive ? _brushActiveColor : _brushDisableColor;
        }
    }
}