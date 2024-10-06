using Beast.Claw.Common;

namespace Beast.Claw.GameCycle
{
    public class GameCycleConfig : Config
    {
        public bool exceptionStopsUpdates;
        public int updateExceptionThreshold;
        public bool preventDuplicatingSubscription;
    }
}