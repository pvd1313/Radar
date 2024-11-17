using System.Collections.Generic;
using Beast.Claw.Common;

namespace Beast.Claw.GameLoop
{
    public struct GameLoopSetup : IValidatable
    {
        private static readonly HashSet<string> ACTION_NAMES = new();
        
        public List<ActionProperties> actions;
        
        public string Validate()
        {
            if (actions == null)
            {
                return $"{nameof(actions)} can not be null";
            }
            
            ACTION_NAMES.Clear();

            for (int i = 0; i < actions.Count; i++)
            {
                ActionProperties operationProperties = actions[i];
                
                if (!operationProperties.IsValid(out string error))
                {
                    return $"Invalid operation at index {i}: " + error;
                }

                if (!ACTION_NAMES.Add(operationProperties.id))
                {
                    return $"Duplicated operation id '{operationProperties.id}'";
                }
            }

            return null;
        }
    }
}