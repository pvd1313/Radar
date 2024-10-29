using System.Collections.Generic;
using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar.Debug
{
    public class DebugConfig : ScriptableConfig
    {
        public GameObject logConsolePrefab;
        public GameObject fpsCounterPrefab;

        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}