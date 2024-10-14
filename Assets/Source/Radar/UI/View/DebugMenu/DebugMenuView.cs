using System.Collections.Generic;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class DebugMenuView : UIView
    {
        public GameObject screen;
        public UIButton openButton;
        public UIButton closeButton;
        public UIButton[] panelButtons;
        public List<InputIconDisplay> inputIcons;
        [Space]
        public DebugItemPanelView itemView;
        public DebugSettingsPanelView settingsView;
    }
}