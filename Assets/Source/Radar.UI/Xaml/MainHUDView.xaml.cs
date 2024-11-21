using Beast.Claw.UI;
using Noesis;

namespace Beast.Radar.UI
{
    public class MainHUDView : BeastControl
    {
        public TextBlock textTime;
        public Button buttonSettings;
        public Button buttonPath;
        public Button buttonMove;
        
        private Brush _brushActiveColor;
        private Brush _brushDisableColor;
        
        private Brush _brushStandImage;
        private Brush _brushMoveImage;
        
        public override void BindElements()
        {
            _find("text_time", out textTime);
            _find("button_settings", out buttonSettings);
            _find("button_path", out buttonPath);
            _find("button_move", out buttonMove);
            
            _brushActiveColor =  (SolidColorBrush)FindResource("Brush.Element.Light.Active");
            _brushDisableColor =  (SolidColorBrush)FindResource("Brush.Element.Light.Disable");
            
            _brushMoveImage =  (ImageBrush)FindResource("Brush.Image.Move");
            _brushStandImage =  (ImageBrush)FindResource("Brush.Image.Stand");
        }
        
        public void ChangeMoveState(bool isMove)
        {
            buttonMove.OpacityMask = isMove ? _brushStandImage : _brushMoveImage;
        }

        public void ChangePathfindingState(bool isActive)
        {
            buttonPath.Background = isActive ? _brushActiveColor : _brushDisableColor;
        }
    }
}