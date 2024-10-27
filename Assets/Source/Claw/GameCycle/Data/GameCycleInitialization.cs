using System.Collections.Generic;



namespace Beast.Claw.GameCycle
{
    public struct GameCycleInitialization : IValidatable
    {
        private static readonly HashSet<string> ACTION_NAMES = new();
        
        public List<ActionInfo> actions;
        
        public string Validate()
        {
            if (actions == null)
            {
                return $"{nameof(actions)} can not be null";
            }
            
            ACTION_NAMES.Clear();

            for (int i = 0; i < actions.Count; i++)
            {
                ActionInfo operationInfo = actions[i];
                
                if (!operationInfo.IsValid(out string error))
                {
                    return $"Invalid operation at index {i}: " + error;
                }

                if (!ACTION_NAMES.Add(operationInfo.id))
                {
                    return $"Duplicated operation id '{operationInfo.id}'";
                }
            }

            return null;
        }
    }
}