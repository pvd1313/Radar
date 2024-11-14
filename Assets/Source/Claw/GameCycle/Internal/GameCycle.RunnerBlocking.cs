using System;
using System.Collections.Generic;
using Beast.Claw.Collection;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.Profiling;

namespace Beast.Claw.GameCycle
{
    public partial class GameCycle
    {
        private struct RunnerBlocking
        {
            public IController owner;
            public HashSet<object> brokenObjects;
            public ManagedFixedList<Runner.Listener> listeners;
            public Dictionary<object, int> objectErrorCount;
            public string operationID;
            public int exceptionThreshold;
            public bool exceptionStopsExecution;
            public bool allLogicIsBroken;
            public Profiler profiler;
            public bool enableClawProfiling;
            public bool flushProfilerToConsole;
            public ProfilerTreeStringBuilder treeStringBuilder;
            
            public void Run()
            {
                bool hasBrokenLogic = brokenObjects.Count > 0;

                bool hasProfiler = profiler != null;
                string listenerName = null;
                Profiler.Handle handle = default;

                for (int i = 0; i < listeners.Count; i++)
                {
                    Runner.Listener listener = listeners[i];
                    
                    if (hasBrokenLogic && brokenObjects.Contains(listener.target))
                    {
                        continue;
                    }
                    
                    if (hasProfiler)
                    {
                        handle = profiler.GetHandle(listener.target);
                        listenerName = listener.target.Meta.name;
                        handle.BeginSample(listenerName);
                    }

                    try
                    {
                        BlockingContext context = default;
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
                        if (hasProfiler)
                        {
                            handle.EndSample(listenerName);
                        }
                    }
                }
                
                if (hasProfiler && enableClawProfiling)
                {
                    profiler.RebuildTree();
                    
                    if (flushProfilerToConsole)
                    {
                        string title = owner.Meta.name + ": " + operationID;
                        string info = treeStringBuilder.ToString(profiler.Reader, title);

                        BDebug.StaticLogInfo(info);
                    }
                }
            }
            
            private void ProcessException(Exception e, Runner.Listener listener, out bool canContinueExecution)
            {
                BDebug.LogException(e);
            
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