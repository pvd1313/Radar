using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Radar.Navigation;

namespace Beast.Radar.City
{
    public class RoadSpawnConfig : ScriptableConfig
    {
        public CellMovementCostConfig buildCost;
        public ERoadSpawnPriority spawnPriority;
        public int maxSegmentLength;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}