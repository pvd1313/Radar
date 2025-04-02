using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Graphic;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public class TerrainRenderConfig : ScriptableConfig
    {
        [Header("Noise Sampler")]
        public float noiseSamplerLOD1;
        public float noiseSamplerLOD0;
        
        [Header("FBM")]
        public RenderTextureData fbmRenderTexture;
        public Texture2DParameters fbmTexture2D;
        
        [Header("Biome")]
        public RenderTextureData biomeRenderTexture;
        public Texture2DParameters biomeTexture2D;
        
        [Header("Color")]
        public RenderTextureData colorRenderTexture;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}