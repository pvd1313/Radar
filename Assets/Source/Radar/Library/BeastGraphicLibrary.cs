using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar
{
    public partial class BeastGraphicLibrary : BeastLibrary
    {
        public Mesh quadMesh;
        
        public Shader worldMapBakeShader;
        public Shader worldMapRenderShader;

        public Texture2D worldDetailNoise;

        public Debug debug;
    }
}