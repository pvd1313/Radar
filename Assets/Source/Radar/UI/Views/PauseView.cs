using UnityEngine;

namespace Beast.Radar.UI
{
    public class PauseView : BeastUIWindowView
    {
        [Space]
        public BeastUIButton newGameButton;
        public BeastUIButton saveButton;
        public BeastUIButton loadButton;
        public BeastUIButton[] closePanelButtons;
    }
}