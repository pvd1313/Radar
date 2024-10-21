using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class DebugMenuView : UIView
    {
        public GameObject screen;
        public TextMeshProUGUI buildVersionText;
        public UIButton openButton;
        public UIButton closeButton;
        public UIButton[] panelButtons;
        public List<InputIconDisplay> inputIcons;
        public DebugItemPanelView itemView;
        public DebugSettingsPanelView settingsView;
    }
}