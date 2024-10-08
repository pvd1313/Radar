using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar
{
    public partial class GraphicLibrary : Library
    {
        public Mesh quadMesh;
        
        public Shader terrainBakeShader;
        public Shader terrainNoiseRenderShader;
        public Shader terrainColorRenderShader;

        public Texture2D worldNoiseTexture;

        public Debug debug;
    }
}