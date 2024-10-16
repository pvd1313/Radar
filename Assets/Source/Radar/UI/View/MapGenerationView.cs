using TMPro;

namespace Beast.Radar.UI
{
    public class MapGenerationView : UIView
    {
        public TextMeshProUGUI seedResult;
        public UIButton nextSeedButton;
        public UIButton previousSeedButton;
        public UISlider forestSlider;
        public UISlider waterSlider;
        public UIButton createMapButton;
        public UIButton closeButton;
        public UIButton[] sizeButtons;
    }
}