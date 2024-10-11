using Beast.Claw.Common;
using Beast.Radar.Graphic.Terrain;
using UnityEngine;

namespace Beast.Radar
{
    public partial class GraphicLibrary : Library
    {
        public Mesh quadMesh;
        
        public Shader terrainBakeShader;
        public Shader terrainDetailShader;
        public Shader terrainColorShader;

        public Shader terrainTileSolver;
        public Shader terrainTileRender;
        public TerrainTileAtlas terrainTileAtlas;

        public Texture2D worldNoiseTexture;

        public Debug debug;
    }
}