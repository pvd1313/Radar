using System.Collections.Generic;
using Beast.Claw.Common;

namespace Beast.Radar.City
{
    public class CitySpawnConfig : ScriptableConfig
    {
        public float majorCityGridSize;
        public float minorCityGridSize;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}