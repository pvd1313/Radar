using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Radar.Terrain;

namespace Beast.Radar.Graphic
{
    public class GraphicConfig : ScriptableConfig
    {
        public TerrainRenderConfig terrain;
        public TileRenderConfig tile;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield return terrain;
            yield return tile;
        }
    }
}