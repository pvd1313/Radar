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
        public DebugItemPanelView itemView;
        public DebugSettingsPanelView settingsView;
    }
}