using System;
using UnityEngine;

namespace Beast.Radar
{
    public partial class GraphicLibrary
    {
        [Serializable]
        public class Debug
        {
#if BEAST_DEVELOPMENT
            public Texture2D bakedTerrain;

            public Texture2D terrainGrid;

            public Shader unlitTextureShader;

            public Shader unlitColorShader;
#endif
        }
    }
}