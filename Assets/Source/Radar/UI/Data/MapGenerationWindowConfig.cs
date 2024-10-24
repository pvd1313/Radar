using System.Collections.Generic;
using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public class MapGenerationWindowConfig : ScriptableConfig
    {
        public SeedData[] seeds; 
        public float previewSizeMultiplier;

        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}