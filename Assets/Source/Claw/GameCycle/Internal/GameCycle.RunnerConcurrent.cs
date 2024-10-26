using System;
using System.Collections.Generic;
using Beast.Claw.Debug;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public partial class GameCycle
    {
        private struct RunnerConcurrent
        {
            public HashSet<object> singleFiredObjects;
            public EOperationFrequency frequency;
            public List<Runner.Listener> listeners;
            public string operationID;
            
            public async UniTask<OperationResult> Run()
            {
                OperationResult result = OperationResult.Success();
                
                foreach (Runner.Listener listener in listeners)
                {
                    if (frequency == EOperationFrequency.OncePerObjectLife &&
                        !singleFiredObjects.Add(listener.target))
                    {
                        continue;
                    }

                    try
                    {
                        OperationContext context = default;

                        result = await listener.concurrent.Invoke(context);

                        if (!result.IsSuccess())
                        {
                            BDebug.LogError(this, $"Error during '{operationID}' ({listener.GetType().FullName})");

                            return result;
                        }
                    }
                    catch (Exception e)
                    {
                        BDebug.LogException(this, e, $"Exception during '{operationID}' ({listener.GetType().FullName})");
                        
                        return OperationResult.Failed();
                    }
                }
                
                return result;
            }
        }
    }
}