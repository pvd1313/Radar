using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Profiling;
using UnityEngine;

namespace Beast.Radar.Debug
{
    public class DebugConfig : ScriptableConfig
    {
#if DEVELOPMENT
        public GameObject logConsolePrefab;
        public GameObject fpsCounterPrefab;
#endif

        public ProfilerConfig profiler;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield return profiler;
            
            yield break;
        }
    }
}