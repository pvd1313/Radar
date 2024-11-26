
using Beast.Claw.UI;
using Noesis;

namespace Beast.Radar.UI
{
    public class MapGenerationWindowView : BeastControl
    {
        private Slider sliderWater;
        
        public override void BindElements()
        {
            _find("slider_water", out sliderWater);
        }

    }
}
