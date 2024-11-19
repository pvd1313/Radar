using Beast.Claw.UI;
using Noesis;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class MainHUDView : BeastControl
    {
        public TextBlock textTime;
        public Button buttonSettings;

        public override void BindElements()
        {
            _find("text_time", out textTime);
            _find("button_settings", out buttonSettings);
        }
   

    }
}