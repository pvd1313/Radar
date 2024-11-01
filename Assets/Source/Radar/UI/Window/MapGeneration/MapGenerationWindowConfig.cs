using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Radar.Terrain;

namespace Beast.Radar.UI
{
    public class MapGenerationWindowConfig : ScriptableConfig
    {
        public MapGenerationSeed[] seeds; 
        public float previewSizeMultiplier;

        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}