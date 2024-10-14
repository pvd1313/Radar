using System.Collections.Generic;
using Beast.Claw.Common;

namespace Beast.Claw.GameCycle
{
    public struct GameCycleParameters : IValidatable
    {
        public HashSet<string> initializationEventIDs;
        public HashSet<string> updateEventIDs;
        
        public string Validate()
        {
            if (initializationEventIDs == null)
            {
                return $"{nameof(initializationEventIDs)} can not be null";
            }

            if (updateEventIDs == null)
            {
                return $"{nameof(updateEventIDs)} can not be null";
            }

            return null;
        }
    }
}