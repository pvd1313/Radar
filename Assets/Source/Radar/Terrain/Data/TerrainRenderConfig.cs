using System;

namespace Beast.Radar.Terrain
{
    [Serializable]
    public class TerrainRenderConfig
    {
        public int tileFadeInPixelSize;
        public int tileMinPixelSize;
        public int tileMaxCount;
        public int tileFadeInCount;
        public float tileDenoiseScale;
    }
}