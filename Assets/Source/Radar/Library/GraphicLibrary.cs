using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Resources;
using Beast.Radar.Terrain;
using UnityEngine;

namespace Beast.Radar
{
    public class GraphicLibrary : ScriptableConfig
    {
        public Mesh quad;

        public TerrainGraphicLibrary terrain;

        public Address<DebugGraphicLibrary> debug;

        public override IEnumerable<IConfig> Bind()
        {
            yield return terrain;
        }
    }
}