using System.Collections.Generic;

namespace Beast.Claw.GameCycle
{
    public struct GameCycleInitialization : IValidatable
    {
        public List<OperationInfo> operations;
        public List<string> updates;
        
        public string Validate()
        {
            if (operations == null)
            {
                return $"{nameof(operations)} can not be null";
            }

            if (updates == null)
            {
                return $"{nameof(updates)} can not be null";
            }

            return null;
        }
    }
}