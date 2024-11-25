using Beast.Claw.UI;
using Noesis;

namespace Beast.Radar.UI
{  
    public class PauseWindowView : BeastControl
    {
        public Button buttonClose;
        public ToggleButton toggleFPS;

        public override void BindElements()
        {
            _find("button_close", out buttonClose);
            _find("toggle_fps", out toggleFPS);
            //_find("text_time", out textTime);
            //_find("button_path", out buttonPath);
            //_find("button_move", out buttonMove);
        }

        
    }
}
