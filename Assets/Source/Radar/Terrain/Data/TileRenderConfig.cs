using System;
using Beast.Claw.Graphic;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    [Serializable]
    public class TileRenderConfig
    {
        public int fadeInPixelSize;
        public int minPixelSize;
        public int maxCount;
        public int fadeInCount;
        public float denoiseScale;
        
        [Header("Textures")]
        public RenderTextureData renderTexture;
        public RenderTextureData solverTexture;
    }
}