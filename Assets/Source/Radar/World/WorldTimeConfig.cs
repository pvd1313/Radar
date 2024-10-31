using System.Collections.Generic;
using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar.World
{
    public class WorldTimeConfig : ScriptableConfig
    {
        public float secondMultiplier;
        [Range(0,1)] public float idleTimeScale;
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}