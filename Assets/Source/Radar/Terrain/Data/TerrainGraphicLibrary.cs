using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    [Serializable]
    public class TerrainGraphicLibrary : IConfig
    {
        public Shader fbmRender;
        public Shader biomeRender;
        public Shader colorRender;

        public Shader tileSolver;
        public Shader tileRender;
        public Shader tileBlit;
        public TerrainAtlasAsset atlas;

        public Texture2D biomeRenderNoise;
        public Texture2D bakerGradient;
        
        public IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}