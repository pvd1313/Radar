using System.Collections.Generic;
using Beast.Claw.Common;

namespace Beast.Radar.GameTime
{
    public class GameTimeConfig : ScriptableConfig
    {
        public float secondMultiplier;
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}