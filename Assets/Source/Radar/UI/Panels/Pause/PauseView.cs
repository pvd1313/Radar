using Beast.Radar.UI.Components;
using Beast.Radar.UI.Panels.Base;
using UnityEngine;

namespace Beast.Radar.UI.Panels.Pause
{
    public class PauseView : BView
    {
        [Space]
        public BButton closePanelButton;
        public BButton newGameButton;
        public BButton saveButton;
        public BButton loadButton;
    }
}