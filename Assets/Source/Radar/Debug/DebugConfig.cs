using System.Collections.Generic;
using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar.Debug
{
    public class DebugConfig : ScriptableConfig
    {
#if DEVELOPMENT
        public GameObject logConsolePrefab;
        public GameObject fpsCounterPrefab;
#endif

        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}