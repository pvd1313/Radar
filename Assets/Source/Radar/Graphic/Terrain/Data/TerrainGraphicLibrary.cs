using System;
using UnityEngine;

namespace Beast.Radar.Graphic.Terrain
{
    [Serializable]
    public class TerrainGraphicLibrary
    {
        public Shader baker;
        public Shader biomeRender;
        public Shader colorRender;

        public Shader tileSolver;
        public Shader tileRender;
        public Shader tileBlit;
        public TerrainAtlasAsset atlas;

        public Texture2D biomeRenderNoise;
        public Texture2D tileSolverNoise;
    }
}