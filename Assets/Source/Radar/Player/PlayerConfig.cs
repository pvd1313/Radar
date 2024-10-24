using System.Collections.Generic;
using Beast.Claw.Common;

namespace Beast.Radar.Player
{
    public class PlayerConfig : ScriptableConfig
    {
        public float moveSpeed;
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}