using System.Collections.Generic;
using Beast.Claw.Common;

namespace Beast.Claw.GameCycle
{
    public class GameCycleConfig : ScriptableConfig
    {
        public bool exceptionStopsUpdates;
        public int updateExceptionThreshold;
        public bool preventDuplicatingSubscription;
        public bool preventExternObjectMethodSubscription;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}