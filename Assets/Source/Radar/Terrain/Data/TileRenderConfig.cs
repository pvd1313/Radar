using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Graphic;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    [Serializable]
    public class TileRenderConfig : IConfig
    {
        public int fadeInPixelSize;
        public int minPixelSize;
        public int maxCount;
        public int fadeInCount;
        
        [Header("Texture")]
        public RenderTextureData renderTexture;
        public RenderTextureData solverTexture;
        
        public IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}