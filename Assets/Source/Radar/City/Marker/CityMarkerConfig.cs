using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Text;

namespace Beast.Radar.City
{
    public class CityMarkerConfig : ScriptableConfig
    {
        public TextBakerConfig textConfig;
        public CityMarkerLod[] iconLods;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}