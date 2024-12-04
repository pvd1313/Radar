using Beast.Claw.UI;
using Noesis;
using Color = UnityEngine.Color;

namespace Beast.Radar.UI
{
    public class GraphyView : BeastControl
    {
        public GraphyFpsAssets FpsAssets { get; private set; }
        public GraphyInformationAssets InformationAssets { get; private set; }
        public GraphyAudioAssets AudioAssets { get; private set; }

        public override void BindElements()
        {
            FpsAssets = new GraphyFpsAssets();
            _find("grid_fps", out FpsAssets.view);
            _find("button_fps", out FpsAssets.button);
            _find("text_fps", out FpsAssets.fps);
            _find("text_ms", out FpsAssets.ms);
            _find("text_avg", out FpsAssets.avg);
            _find("text_one_percent", out FpsAssets.onePercentFps);
            _find("text_zero_one_percent", out FpsAssets.textZero1PercentFps);
            _find("texture_fps", out FpsAssets.display);
            
            InformationAssets = new GraphyInformationAssets();
            _find("grid_information", out InformationAssets.view);
            _find("button_information", out InformationAssets.button);
            _find("text_graphicsDeviceVersion", out InformationAssets.graphicsDeviceVersionText);
            _find("text_processorType", out InformationAssets.processorTypeText);
            _find("text_operatingSystem", out InformationAssets.operatingSystemText);
            _find("text_graphicsDeviceName", out InformationAssets.graphicsDeviceNameText);
            _find("text_graphicsMemorySize", out InformationAssets.graphicsMemorySizeText);
            _find("text_screenResolution", out InformationAssets.screenResolutionText);
            _find("text_gameWindowResolution", out InformationAssets.gameWindowResolutionText);
            _find("text_systemMemory", out InformationAssets.systemMemoryText);

            AudioAssets = new GraphyAudioAssets();
            _find("grid_audio", out AudioAssets.view);
            _find("button_audio", out AudioAssets.button);
            _find("text_db", out AudioAssets.db);
            _find("texture_audio_current", out AudioAssets.displayCurrent);
            _find("texture_audio_previous", out AudioAssets.displayPrevious);
        }

        public void InitializeFpsBrushes(Color good, Color caution, Color critical)
        {
            FpsAssets.brushes = new GraphyBrushes()
            {
                good = new SolidColorBrush(good.ToNoesis()),
                caution = new SolidColorBrush(caution.ToNoesis()),
                critical = new SolidColorBrush(critical.ToNoesis())
            };
        }
        
    }
}