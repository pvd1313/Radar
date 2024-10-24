using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Graphic;

namespace Beast.Radar.Terrain
{
    [Serializable]
    public class TerrainRenderConfig : IConfig
    {
        public float noiseSamplerLOD1;
        public float noiseSamplerLOD0;
        public RenderTextureData bakerRenderTexture;
        public Texture2DParameters bakerTexture2D;
        public RenderTextureData biomeTexture;
        public RenderTextureData colorTexture;
        
        public IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}