using System.Collections.Generic;
using Beast.Claw.Common;

namespace Beast.Radar.City
{
    public class CitySpriteConfig : ScriptableConfig
    {
        public CitySpriteData[] sprites;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}