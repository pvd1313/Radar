using System;
using System.Collections.Generic;
using Beast.Claw.Collections;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.Profiling;
using Beast.Claw.ProgressTracker;
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
            public IProgressTracker progressTracker;

            private NativeFixedList<short> _passedActors;
            
            public async UniTask<EActionStatus> Run()
            {
                EActionStatus result = EActionStatus.Success;

                _passedActors = activeActorsContainer.data;

                _passedActors.Clear();

                bool hasProgressTracker = progressTracker != null;

                for (int actorIndex = 0; actorIndex < allActors.Count; actorIndex++)
                {
                    Runner.Actor actor = allActors[actorIndex];
                    
                    if (frequency == EActionFrequency.OncePerActorLife &&
                        !singleFiredObjects.Add(actor.target))
                    {
                        continue;
                    }

                    if (hasProgressTracker)
                    {
                        await progressTracker.Next(actor.target.Meta.shortName);
                    }

                    string actorFullName = actor.target.Meta.fullName;
                    
                    profiler.BeginSample(actorFullName);

                    try
                    {
                        ConcurrentContext context = default;

                        await actor.concurrent.Invoke(context);

                        _passedActors.Add((short)actorIndex);
                    }
                    catch (Exception e)
                    {
                        BDebug.LogException(e);

                        return EActionStatus.Failed;
                    }
                    finally
                    {
                       profiler.EndSample(actorFullName);
                    }
                }

                activeActorsContainer.data = _passedActors;

                return result;
            }
        }
    }
}