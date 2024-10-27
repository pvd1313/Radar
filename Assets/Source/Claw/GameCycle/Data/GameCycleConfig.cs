using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Profiling;

namespace Beast.Claw.GameCycle
{
    public class GameCycleConfig : ScriptableConfig
    {
        public bool exceptionStopsUpdates;
        public int updateExceptionThreshold;
        public bool preventDuplicateSubscription;
        public bool preventExternSubscription;
        public ProfilerConfig profiler;
        public bool logConcurrentAction;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}