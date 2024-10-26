using System.Collections.Generic;

namespace Beast.Claw.GameCycle
{
    public struct GameCycleData : IValidatable
    {
        public List<OperationInfo> operationEvents;
        public List<string> updateEvents;
        
        public string Validate()
        {
            if (operationEvents == null)
            {
                return $"{nameof(operationEvents)} can not be null";
            }

            if (updateEvents == null)
            {
                return $"{nameof(updateEvents)} can not be null";
            }

            return null;
        }
    }
}