using Beast.Claw.UI;
using Beast.Radar.UI.Graphy;
using Noesis;
using UnityEngine;
using Color = UnityEngine.Color;

namespace Beast.Radar.UI
{
    public class GraphyView : BeastControl
    {
        public GraphyBrushes Brushes { get; private set; }
        public GraphyFpsTextAssets FpsTextAssets { get; private set; }

        private Rectangle textureFps;

        public override void BindElements()
        {
            IsHitTestVisible = false;

            FpsTextAssets = new GraphyFpsTextAssets();

            _find("text_fps", out FpsTextAssets.fps);
            _find("text_ms", out FpsTextAssets.ms);
            _find("text_avg", out FpsTextAssets.avg);
            _find("text_one_percent", out FpsTextAssets.onePercentFps);
            _find("text_zero_one_percent", out FpsTextAssets.textZero1PercentFps);
            _find("texture_fps", out textureFps);
        }

        public void InitializeBrushes(Color good, Color caution, Color critical)
        {
            Brushes = new GraphyBrushes()
            {
                good = new SolidColorBrush(good.ToNoesis()),
                caution = new SolidColorBrush(caution.ToNoesis()),
                critical = new SolidColorBrush(critical.ToNoesis())
            };
        }

        public void SetFpsTexture(Material shaderGraphMaterial)
        {
            TextureSource texture = new(shaderGraphMaterial.mainTexture);
            ImageBrush brush = new(texture);
            textureFps.Fill = brush;
        }
    }
}