using Beast.Claw.Common;

namespace Beast.Claw.GameCycle
{
    public class BeastGameCycleConfig : BeastConfig
    {
        public bool exceptionStopsUpdates;
        public int updateExceptionThreshold;
        public bool preventDuplicatingSubscription;
    }
}