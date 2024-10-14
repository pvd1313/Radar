using System;
using UnityEngine;

namespace Beast.Radar.UI
{
    [Serializable]
    public class DebugSettingsPanelView
    {
        public GameObject body;
        public UIButton[] timeScaleButtons;
        public UIButton[] frameRateButtons;
    }
}