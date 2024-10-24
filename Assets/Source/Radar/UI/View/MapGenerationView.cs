using TMPro;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class MapGenerationView : UIView
    {
        public TextMeshProUGUI seedResult;
        public UIStepButtons seedButtons;
        public UISlider forestSlider;
        public UISlider waterSlider;
        public UISlider biomeSlider;
        public UIButton createMapButton;
        public UIButton closeButton;
        public RawImage mapImage;
        public RadioButton[] sizeButtons;
    }
}