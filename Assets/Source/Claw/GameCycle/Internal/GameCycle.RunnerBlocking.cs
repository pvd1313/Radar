using System;
using System.Collections.Generic;
using Beast.Claw.Debug;
using Beast.Claw.Profiling;

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
            public GameCycleConfig config;
            public RunnerProfiler profiler;
            
            public void Run()
            {
                bool hasBrokenLogic = brokenObjects.Count > 0;
                
                Profiler.Handle handle = default;
                string objectName = null;

                for (int i = 0; i < listeners.Count; i++)
                {
                    Runner.Listener listener = listeners[i];
                    
                    if (hasBrokenLogic && brokenObjects.Contains(listener.target))
                    {
                        continue;
                    }
                    
                    if (profiler.isInitialized)
                    {
                        objectName = profiler.objectNames[i];
                        handle = profiler.profiler.GetHandle(listener.target);
                        handle.BeginSample(objectName);
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
                    finally
                    {
                        if (profiler.isInitialized)
                        {
                            handle.EndSample(objectName);
                        }
                    }
                }
                
                if (profiler.isInitialized && config.profiler.enableClawProfiling)
                {
                    profiler.profiler.RebuildTree();
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