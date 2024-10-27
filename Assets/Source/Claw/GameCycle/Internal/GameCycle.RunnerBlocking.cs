using System;
using System.Collections.Generic;
using Beast.Claw.Collection;
using Beast.Claw.Debug;
using Beast.Claw.Profiling;

namespace Beast.Claw.GameCycle
{
    public partial class GameCycle
    {
        private struct RunnerBlocking
        {
            public HashSet<object> brokenObjects;
            public ItemBuffer<Runner.Listener> listeners;
            public Dictionary<object, int> objectErrorCount;
            public string operationID;
            public int exceptionThreshold;
            public bool exceptionStopsExecution;
            public bool allLogicIsBroken;
            public RunnerProfiler profiler;
            public bool enableClawProfiling;
            
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
                    
                    if (profiler.IsInitialized)
                    {
                        objectName = profiler.ObjectNames[i];
                        handle = profiler.Profiler.GetHandle(listener.target);
                        handle.BeginSample(objectName);
                    }

                    try
                    {
                        ActionContext context = default;
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
                        if (profiler.IsInitialized)
                        {
                            handle.EndSample(objectName);
                        }
                    }
                }
                
                if (profiler.IsInitialized && enableClawProfiling)
                {
                    profiler.Profiler.RebuildTree();
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