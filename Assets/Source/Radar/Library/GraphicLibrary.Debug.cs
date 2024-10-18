using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar
{
    public partial class GraphicLibrary
    {
        [Serializable]
        public class DebugGraphicLibrary : IConfig
        {
#if DEVELOPMENT
            public Texture2D bakedTerrain;
            public Texture2D terrainGrid;

            public Shader unlitTextureShader;
            public Shader unlitColorShader;

            public float magic0;
            public float magic1;
            public float magic2;

            public bool toggle0;
            public bool toggle1;
            public bool toggle2;
#endif
            
            public IEnumerable<IConfig> Bind()
            {
                yield break;
            }
        }
    }
}