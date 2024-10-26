using System.Collections.Generic;



namespace Beast.Claw.GameCycle
{
    public struct GameCycleInitialization : IValidatable
    {
        private static readonly HashSet<string> OPERATION_NAMES = new();
        
        public List<OperationInfo> operations;
        
        public string Validate()
        {
            if (operations == null)
            {
                return $"{nameof(operations)} can not be null";
            }
            
            OPERATION_NAMES.Clear();

            for (int i = 0; i < operations.Count; i++)
            {
                OperationInfo operationInfo = operations[i];
                
                if (!operationInfo.IsValid(out string error))
                {
                    return $"Invalid operation at index {i}: " + error;
                }

                if (!OPERATION_NAMES.Add(operationInfo.id))
                {
                    return $"Duplicated operation id '{operationInfo.id}'";
                }
            }

            return null;
        }
    }
}