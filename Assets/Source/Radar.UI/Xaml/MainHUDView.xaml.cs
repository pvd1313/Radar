using Beast.Claw.Common;
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

        private Cache<int> _playerMovementCache;
        private Cache<bool> _autoPathFindingActiveCache;

        public override void BindElements()
        {
            _find("text_time", out textTime);
            _find("button_settings", out buttonSettings);
            _find("button_path", out autoPathFindingButton);
            _find("button_move", out buttonMove);
            
            _brushActiveColor = (SolidColorBrush)FindResource("Brush.Element.Light.Normal");
            _brushDisableColor = (SolidColorBrush)FindResource("Brush.Element.Light.Disabled");
            
            _brushMoveImage = (ImageBrush)FindResource("Brush.Image.Move");
            _brushStandImage = (ImageBrush)FindResource("Brush.Image.Stand");
        }
        
        public void SetPlayerMovementState(bool hasTarget, bool isMoving)
        {
            int playerMovement = !hasTarget ? 0 : (isMoving ? 1 : 2);

            if (_playerMovementCache.Update(playerMovement))
            {
                buttonMove.OpacityMask = isMoving ? _brushMoveImage : _brushStandImage;
                buttonMove.Visibility = hasTarget ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public void SetAutoPathFindingActive(bool isActive)
        {
            if (_autoPathFindingActiveCache.Update(isActive))
            {
                autoPathFindingButton.Background = isActive ? _brushActiveColor : _brushDisableColor;
            }
        }
    }
}