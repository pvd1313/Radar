using Noesis;
using UnityEngine;
using Grid = Noesis.Grid;

namespace Beast.Radar.UI
{
    public class GraphyFpsAssets
    {
        public ToggleButton button;
        public Grid view;
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