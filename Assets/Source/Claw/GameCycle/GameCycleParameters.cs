using System.Collections.Generic;
using Beast.Claw.Common;

namespace Beast.Claw.GameCycle
{
    public struct GameCycleParameters : IValidatable
    {
        public HashSet<string> initializationEventIDs;
        public HashSet<string> updateEventIDs;
        
        public bool IsValid(out string errorMessage)
        {
            errorMessage = null;
            
            if (initializationEventIDs == null)
            {
                errorMessage = $"{nameof(initializationEventIDs)} can not be null";
                return false;
            }

            if (updateEventIDs == null)
            {
                errorMessage = $"{nameof(updateEventIDs)} can not be null";
                return false;
            }

            return true;
        }
    }
}