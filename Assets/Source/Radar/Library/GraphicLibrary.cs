using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Radar.Terrain;
using UnityEngine;

namespace Beast.Radar
{
    public partial class GraphicLibrary : ScriptableConfig
    {
        public Mesh quad;

        public TerrainGraphicLibrary terrain;

        public DebugGraphicLibrary debug;


        public override IEnumerable<IConfig> Bind()
        {
            yield return terrain;
            yield return debug;
        }
    }
}