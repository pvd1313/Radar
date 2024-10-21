using System.Collections.Generic;
using Beast.Claw.Common;

namespace Beast.Radar.Terrain
{
    public class MapGenerationWindowConfig : ScriptableConfig
    {
        public SeedData[] seeds;
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}