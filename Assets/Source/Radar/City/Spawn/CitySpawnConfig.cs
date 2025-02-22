using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Radar.Navigation;

namespace Beast.Radar.City
{
    public class CitySpawnConfig : ScriptableConfig
    {
        public float majorCityGridSize;
        public float minorCityGridSize;
        public CellMovementCostConfig roadBuildCost;
        public ERoadSpawnPriority majorRoadSpawnPriority;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}