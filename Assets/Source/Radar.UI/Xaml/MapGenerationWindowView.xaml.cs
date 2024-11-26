using Beast.Claw.UI;
using Noesis;

namespace Beast.Radar.UI
{
    public class MapGenerationWindowView : BeastControl
    {
        public Rectangle texture;

        public Button buttonNextSeed;
        public Button buttonPreviousSeed;
        public TextBlock textSeed;

        public RadioButton buttonSize512;
        public RadioButton buttonSize1024;
        public RadioButton buttonSize2048;
        public RadioButton buttonSize4096;
        public RadioButton[] buttonsSize;

        public Slider sliderWater;
        public Slider sliderForest;
        public Slider sliderBiomeSize;

        public Button buttonCreate;
        public Button buttonClose;
        
        public override void BindElements()
        {
            _find("rect_texture", out texture);

            _find("button_next_seed", out buttonNextSeed);
            _find("button_previous_seed", out buttonPreviousSeed);
            _find("text_seed", out textSeed);

            _find("radio_button_512", out buttonSize512);
            _find("radio_button_1024", out buttonSize1024);
            _find("radio_button_2048", out buttonSize2048);
            _find("radio_button_4096", out buttonSize4096);
            buttonsSize = new[]
            {
                buttonSize512,
                buttonSize1024,
                buttonSize2048,
                buttonSize4096,
            };
            
            _find("slider_water", out sliderWater);
            _find("slider_forest", out sliderForest);
            _find("slider_biome_size", out sliderBiomeSize);

            _find("button_create", out buttonCreate);
            _find("button_close", out buttonClose);
        }
    }
}