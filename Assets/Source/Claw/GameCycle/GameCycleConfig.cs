using Beast.Claw.Common;

namespace Beast.Claw.GameCycle
{
    public class GameCycleConfig : ScriptableConfig
    {
        public bool exceptionStopsUpdates;
        public int updateExceptionThreshold;
        public bool preventDuplicatingSubscription;
    }
}