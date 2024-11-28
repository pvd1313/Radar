using System;
using Beast.Claw.Collections;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.Profiling;
using Unity.Collections;

namespace Beast.Claw.GameLoop
{
    public partial class GameLoop
    {
        private struct RunnerBlocking
        {
            public IController owner;
            public ManagedFixedList<Runner.Actor> allActors;
            public NativeHashMap<int, int> actorErrorCounters;
            public string operationID;
            public int exceptionThreshold;
            public bool exceptionStopsExecution;
            public Profiler profiler;
            public bool enableClawProfiling;
            public bool flushProfilerToConsole;
            public ProfilerTreeStringBuilder treeStringBuilder;
            public NativeFixedList<short> passedActors;

            public void Run()
            {
                passedActors.Clear();

                bool hasProfiler = profiler != null;
                string listenerName = null;
                Profiler.Handle handle = default;

                for (int actorIndex = 0; actorIndex < allActors.Count; actorIndex++)
                {
                    Runner.Actor actor = allActors[actorIndex];
                    
                    if (hasProfiler)
                    {
                        handle = profiler.GetHandle(actor.target);
                        listenerName = actor.target.Meta.name;
                        handle.BeginSample(listenerName);
                    }

                    try
                    {
                        BlockingContext context = default;

                        actor.blocking.Invoke(context);

                        passedActors.Add((short)actorIndex);
                    }
                    catch (ActionAbortedException)
                    {
                        
                    }
                    catch (Exception e)
                    {
                        ProcessException(e, actor, actorIndex, out bool canContinue);

                        if (!canContinue)
                        {
                            passedActors.Clear();

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
            
            private void ProcessException(
                Exception e, 
                Runner.Actor actor,
                int actorIndex,
                out bool allIsBroken)
            {
                BDebug.LogException(e);

                int actorGuid = actor.target.Meta.guid;
                
                actorErrorCounters.TryGetValue(actorGuid, out int errorCount);
                errorCount++;
                actorErrorCounters[actor.target.Meta.guid] = errorCount;

                if (errorCount <= exceptionThreshold)
                {
                    passedActors.Add((short)actorIndex);
                }
                else
                {
                    actorErrorCounters.Remove(actorGuid);
                }

                if (exceptionStopsExecution)
                {
                    allIsBroken = false;
                }
                else
                {
                    allIsBroken = true;
                }
            }
        }
    }
}