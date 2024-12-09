using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Radar.Navigation;

namespace Beast.Radar.City
{
    public class CitySpawnConfig : ScriptableConfig
    {
        public float majorCityGridScale;
        public float minorCityGridScale;
        public CellMovementCostConfig roadBuildCost;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}