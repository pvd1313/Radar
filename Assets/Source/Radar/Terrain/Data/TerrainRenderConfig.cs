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
        public RenderTextureData fbmRenderTexture;
        public Texture2DParameters fbmTexture2D;
        public RenderTextureData biomeRenderTexture;
        public Texture2DParameters biomeTexture2D;
        public RenderTextureData colorTexture;
        
        public IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}