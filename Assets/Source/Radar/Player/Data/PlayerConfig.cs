using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Radar.Navigation;

namespace Beast.Radar.Player
{
    public class PlayerConfig : ScriptableConfig
    {
        public float movementSpeedKilometresPerHour;
        public int pathBufferCapacity;
        public CellMovementCostConfig cellMovementCostConfig;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}