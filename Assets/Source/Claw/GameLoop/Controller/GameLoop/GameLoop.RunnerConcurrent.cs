using System;
using System.Collections.Generic;
using Beast.Claw.Collections;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.Profiling;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameLoop
{
    public partial class GameLoop
    {
        private struct RunnerConcurrent
        {
            public HashSet<object> singleFiredObjects;
            public EActionFrequency frequency;
            public ManagedFixedList<Runner.Actor> allActors;
            public BricksProfiler.Handle profiler;
            public Container<NativeFixedList<short>> activeActorsContainer;

            private NativeFixedList<short> _passedActors;
            
            public async UniTask<EActionStatus> Run()
            {
                EActionStatus result = EActionStatus.Success;

                _passedActors = activeActorsContainer.data;

                _passedActors.Clear();

                for (int actorIndex = 0; actorIndex < allActors.Count; actorIndex++)
                {
                    Runner.Actor actor = allActors[actorIndex];
                    
                    if (frequency == EActionFrequency.OncePerActorLife &&
                        !singleFiredObjects.Add(actor.target))
                    {
                        continue;
                    }

                    string actorName = actor.target.Meta.name;
                    profiler.BeginSample(actorName);

                    try
                    {
                        ConcurrentContext context = default;

                        await actor.concurrent.Invoke(context);

                        _passedActors.Add((short)actorIndex);
                    }
                    catch (ActionAbortedException)
                    {
                        
                    }
                    catch (Exception e)
                    {
                        BDebug.LogException(e);

                        return EActionStatus.Failed;
                    }
                    finally
                    {
                       profiler.EndSample(actorName);
                    }
                }

                activeActorsContainer.data = _passedActors;

                return result;
            }
        }
    }
}