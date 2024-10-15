using Beast.Claw.Common;
using Beast.Radar.Graphic.Terrain;
using UnityEngine;

namespace Beast.Radar
{
    public partial class GraphicLibrary : ScriptableConfig
    {
        public Mesh quadMesh;
        
        public Shader terrainBakeShader;
        public Shader terrainIndexShader;
        public Shader terrainColorShader;

        public Shader terrainTileSolver;
        public Shader terrainTileRender;
        public TerrainTileAtlasAsset terrainTileAtlas;

        public Texture2D terrainIndexNoiseTexture;
        public Texture2D terrainTileSolverNoiseTexture;

        public Debug debug;
    }
}