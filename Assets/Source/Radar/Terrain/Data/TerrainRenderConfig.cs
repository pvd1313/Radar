using System;
using Beast.Claw.Graphic;

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

        public RenderTextureData bakedTexture;
        public RenderTextureData colorTexture;
        public RenderTextureData biomeTexture;
        public RenderTextureData tileRenderTexture;
        public RenderTextureData tileSolverTexture;
    }
}