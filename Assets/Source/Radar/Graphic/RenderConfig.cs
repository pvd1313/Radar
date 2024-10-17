using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Radar.Terrain;

namespace Beast.Radar.Graphic
{
    public class RenderConfig : ScriptableConfig
    {
        public TerrainRenderConfig terrain;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield return terrain;
        }
    }
}