using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Graphic;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    [Serializable]
    public class TerrainRenderConfig : IConfig
    {
        public TileRenderConfig tile;

        [Header("Textures")] 
        public RenderTextureData bakedTexture;
        public RenderTextureData biomeTexture;
        public RenderTextureData colorTexture;
        
        public IEnumerable<IConfig> Bind()
        {
            yield return tile;
        }
    }
}