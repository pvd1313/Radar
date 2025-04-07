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
            public ManagedFixedList<Runner.Actor> allActors;
            public NativeHashMap<int, int> actorErrorCounters;
            public int exceptionThreshold;
            public bool exceptionStopsExecution;
            public NativeFixedList<short> activeActors;
            public BricksProfiler.Handle profiler;

            public void Run()
            {
                activeActors.Clear();

                for (int actorIndex = 0; actorIndex < allActors.Count; actorIndex++)
                {
                    Runner.Actor actor = allActors[actorIndex];

                    string actorName = actor.target.Meta.name;
                    
                    profiler.BeginSample(actorName);

                    try
                    {
                        BlockingContext context = default;

                        actor.blocking.Invoke(context);

                        activeActors.Add((short)actorIndex);
                    }
                    catch (ActionAbortedException)
                    {
                        
                    }
                    catch (Exception e)
                    {
                        ProcessException(e, actor, actorIndex, out bool canContinue);

                        if (!canContinue)
                        {
                            activeActors.Clear();

                            break;
                        }
                    }
                    finally
                    {
                       profiler.EndSample(actorName);
                    }
                }
            }
            
            private void ProcessException(Exception e, Runner.Actor actor, int actorIndex, out bool allIsBroken)
            {
                BDebug.LogException(e);

                int actorGuid = actor.target.Meta.guid;
                
                actorErrorCounters.TryGetValue(actorGuid, out int errorCount);
                errorCount++;
                actorErrorCounters[actor.target.Meta.guid] = errorCount;

                if (errorCount <= exceptionThreshold)
                {
                    activeActors.Add((short)actorIndex);
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