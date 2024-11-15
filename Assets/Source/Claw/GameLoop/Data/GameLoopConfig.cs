using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Profiling;

namespace Beast.Claw.GameLoop
{
    public class GameLoopConfig : ScriptableConfig
    {
        public bool exceptionStopsUpdates;
        public int updateExceptionThreshold;
        public ProfilerConfig profiler;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}