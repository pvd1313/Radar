using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public class TerrainAtlasAsset : ScriptableContainer<TerrainAtlas>
    {
        public Texture2D ColorTexture => data.colorTexture;
        public Texture2D MainTexture => data.mainTexture;
        public Texture2D ConfigTexture => data.configTexture;

    }
}