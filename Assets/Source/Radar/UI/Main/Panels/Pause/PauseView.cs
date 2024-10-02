using UnityEngine;

namespace Beast.Radar.UI
{
    public class PauseView : UIPanelView
    {
        [Space]
        public UIButtonView newGameButton;
        public UIButtonView saveButton;
        public UIButtonView loadButton;
        public UIButtonView[] closePanelButtons;
    }
}