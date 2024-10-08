using Beast.Radar.UI.Debug;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class DebugMenuView : UIView
    {
        public GameObject screen;
        public UIButton openButton;
        public UIButton closeButton;
        public UIButton[] folderButtons;
        public DebugItemFolderView itemView;
        public DebugSettingsFolderView settingsView;
    }
}