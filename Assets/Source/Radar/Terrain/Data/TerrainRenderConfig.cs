using System;
using Beast.Claw.Graphic;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    [Serializable]
    public class TerrainRenderConfig
    {
        public TileRenderConfig tile;

        [Header("Textures")] 
        public RenderTextureData bakedTexture;
        public RenderTextureData biomeTexture;
        public RenderTextureData colorTexture;
    }
}