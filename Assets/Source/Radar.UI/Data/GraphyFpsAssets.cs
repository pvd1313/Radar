using Noesis;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class GraphyFpsAssets
    {
        public TextBlock fps;
        public TextBlock ms;
        public TextBlock avg;
        public TextBlock onePercentFps;
        public TextBlock textZero1PercentFps;
        public GraphyBrushes brushes;
        
        public Rectangle textureFps;
        
        public void SetFpsTexture(Material shaderGraphMaterial)
        {
            TextureSource texture = new(shaderGraphMaterial.mainTexture);
            ImageBrush brush = new(texture);
            textureFps.Fill = brush;
        }
    }
}