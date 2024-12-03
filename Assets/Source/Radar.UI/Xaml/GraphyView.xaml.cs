using Beast.Claw.UI;
using Noesis;
using Color = UnityEngine.Color;

namespace Beast.Radar.UI
{
    public class GraphyView : BeastControl
    {
        public GraphyFpsAssets FpsAssets { get; private set; }
        public GraphyAdvancedAssets AdvancedAssets { get; private set; }
        

        public override void BindElements()
        {
            IsHitTestVisible = false;

            FpsAssets = new GraphyFpsAssets();
            _find("text_fps", out FpsAssets.fps);
            _find("text_ms", out FpsAssets.ms);
            _find("text_avg", out FpsAssets.avg);
            _find("text_one_percent", out FpsAssets.onePercentFps);
            _find("text_zero_one_percent", out FpsAssets.textZero1PercentFps);
            _find("texture_fps", out FpsAssets.textureFps);
            
            AdvancedAssets = new GraphyAdvancedAssets();
            _find("grid_advanced", out AdvancedAssets.view);
            _find("text_graphicsDeviceVersion", out AdvancedAssets.graphicsDeviceVersionText);
            _find("text_processorType", out AdvancedAssets.processorTypeText);
            _find("text_operatingSystem", out AdvancedAssets.operatingSystemText);
            _find("text_graphicsDeviceName", out AdvancedAssets.graphicsDeviceNameText);
            _find("text_graphicsMemorySize", out AdvancedAssets.graphicsMemorySizeText);
            _find("text_screenResolution", out AdvancedAssets.screenResolutionText);
            _find("text_gameWindowResolution", out AdvancedAssets.gameWindowResolutionText);
            _find("text_systemMemory", out AdvancedAssets.systemMemoryText);
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