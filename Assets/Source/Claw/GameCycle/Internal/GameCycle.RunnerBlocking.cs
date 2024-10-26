using System;
using System.Collections.Generic;
using Beast.Claw.Debug;

namespace Beast.Claw.GameCycle
{
    public partial class GameCycle
    {
        private struct RunnerBlocking
        {
            public HashSet<object> brokenObjects;
            public List<Runner.Listener> listeners;
            public Dictionary<object, int> objectErrorCount;
            public string operationID;
            public int exceptionThreshold;
            public bool exceptionStopsExecution;
            public bool allLogicIsBroken;
            
            public void Run()
            {
                bool hasBrokenLogic = brokenObjects.Count > 0;
                
                foreach (Runner.Listener listener in listeners)
                {
                    if (hasBrokenLogic && brokenObjects.Contains(listener.target))
                    {
                        continue;
                    }

                    try
                    {
                        OperationContext context = default;
                        listener.blocking.Invoke(context);
                    }
                    catch (Exception e)
                    {
                        ProcessException(e, listener, out bool canContinue);

                        if (!canContinue)
                        {
                            allLogicIsBroken = true;
                            
                            break;
                        }
                    }
                }
            }
            
            
            private void ProcessException(Exception e, Runner.Listener listener, out bool canContinueExecution)
            {
                BDebug.LogException(this, e, listener.GetType().FullName + "." + operationID + "()");
            
                objectErrorCount.TryGetValue(listener.target, out int errorCount);
                errorCount++;
                objectErrorCount[listener.target] = errorCount;
            
                if (errorCount > exceptionThreshold)
                {
                    brokenObjects.Add(listener.target);
                }
            
                if (exceptionStopsExecution)
                {
                    canContinueExecution = false;
                }
                else
                {
                    canContinueExecution = true;
                }
            }
        }
    }
}