using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Text.Baking;
using UnityEngine;

namespace Beast.Radar.City
{
    public class CityIconConfig : ScriptableConfig
    {
        public TextBakerConfig textConfig;
        public CityIconLod[] iconLods;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}