using System.Collections.Generic;
using Beast.Claw.Common;

namespace Beast.Radar.GameTime
{
    public class GameTimeConfig : ScriptableConfig
    {
        public UnitySerializableDateTime initialDate;

        public float playerIdleTimeScaleMinutesPerSecond;
        public float playerWalkingTimeScaleMinutesPerSecond;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}