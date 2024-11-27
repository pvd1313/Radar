using Beast.Claw.UI;
using Noesis;

namespace Beast.Radar.UI
{  
    public class PauseWindowView : BeastControl
    {
        public Button buttonClose;
        public ToggleButton toggleFPS;
        public ToggleButton toggleGraphy;

        public TextBlock textMovement;
        public Button buttonNextMovement;
        public Button buttonPreviousMovement;

        public Button buttonLoadGame;
        public Button buttonSaveGame;
        public Button buttonNewGame;
        
        public Button buttonMainMenu;
        
        public override void BindElements()
        {
            _find("button_close", out buttonClose);
            _find("toggle_fps", out toggleFPS);
            _find("toggle_graphy", out toggleGraphy);
            _find("text_movement", out textMovement);
            _find("button_next_movement", out buttonNextMovement);
            _find("button_previous_movement", out buttonPreviousMovement);
            _find("button_save", out buttonSaveGame);
            _find("button_load", out buttonLoadGame);
            _find("button_new_game", out buttonNewGame);
            _find("button_main_menu", out buttonMainMenu);
        }
    }
}
