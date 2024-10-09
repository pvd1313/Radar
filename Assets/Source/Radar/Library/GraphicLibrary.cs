using Beast.Claw.Common;
using Beast.Radar.Graphic.Terrain;
using UnityEngine;

namespace Beast.Radar
{
    public partial class GraphicLibrary : Library
    {
        public Mesh quadMesh;
        
        public Shader terrainBakeShader;
        public Shader terrainNoiseShader;
        public Shader terrainColorShader;

        public Shader isometricSolverShader;
        public Shader isometricRenderShader;
        public TerrainTileAtlas terrainTileAtlas;

        public Texture2D worldNoiseTexture;

        public Debug debug;
    }
}