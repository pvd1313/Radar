using System.Collections.Generic;
using Beast.Claw.Common;

namespace Beast.Radar.City
{
    public class CityNameConfig : ScriptableConfig
    {
        public string[] majorNames;
        public string[] minorNamePart0;
        public string[] minorNamePart1;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}