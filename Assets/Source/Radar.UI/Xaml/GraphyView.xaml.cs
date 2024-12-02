using Beast.Claw.UI;
using Beast.Radar.UI.Graphy;
using Noesis;
using UnityEngine;

namespace Beast.Radar.UI
{
    public  class GraphyView : BeastControl
    {
        public GraphyFpsTextAssets textAssets;
        private Rectangle textureFps;

        public override void BindElements()
        {
            IsHitTestVisible = false;

            textAssets = new GraphyFpsTextAssets();
            _find("text_fps", out  textAssets.fps);
            _find("text_ms", out  textAssets.ms);
            _find("text_avg", out  textAssets.avg);
            _find("text_one_percent", out  textAssets.onePercentFps);
            _find("text_zero_one_percent", out  textAssets.textZero1PercentFps);
            _find("texture_fps", out textureFps);
        }

        
        public void SetFpsTexture(Material shaderGraphMaterial)
        {
            TextureSource texture = new TextureSource(shaderGraphMaterial.mainTexture);
            ImageBrush brush = new ImageBrush(texture);
            brush.Stretch = Stretch.Fill;
            textureFps.Fill = brush;
        }
    }
}
