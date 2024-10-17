using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Graphic;

namespace Beast.Radar.Terrain
{
    [Serializable]
    public class TerrainRenderConfig : IConfig
    {
        public RenderTextureData bakedTexture;
        public RenderTextureData biomeTexture;
        public RenderTextureData colorTexture;
        
        public IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}