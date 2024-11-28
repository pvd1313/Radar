using System.Collections.Generic;
using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public class TerrainAtlasAsset : ScriptableContainer<TerrainAtlas>, IConfig
    {
        public Texture2D ColorTexture => data.colorTexture;
        public Texture2D MainTexture => data.mainTexture;
        public Texture2D ConfigTexture => data.configTexture;
        public Texture2D IndexTexture => data.indexTexture;

        public IEnumerable<IConfig> Bind()
        {
            yield return data;
        }
    }
}