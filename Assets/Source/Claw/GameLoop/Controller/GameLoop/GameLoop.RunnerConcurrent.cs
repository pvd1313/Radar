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
            public IController owner;
            public HashSet<object> singleFiredObjects;
            public EActionFrequency frequency;
            public ManagedFixedList<Runner.Actor> allActors;
            public string operationID;
            public Profiler profiler;
            public ProfilerTreeStringBuilder treeStringBuilder;
            public Container<NativeFixedList<short>> passedActorContainer;
            public bool enableClawProfiling;
            public bool flushProfilerToConsole;

            private NativeFixedList<short> _passedActors;
            
            public async UniTask<EActionStatus> Run()
            {
                EActionStatus result = EActionStatus.Success;

                _passedActors = passedActorContainer.data;

                _passedActors.Clear();

                bool hasProfiler = profiler != null;
                
                Profiler.Handle handle = default;
                string listenerName = null;

                for (int actorIndex = 0; actorIndex < allActors.Count; actorIndex++)
                {
                    Runner.Actor actor = allActors[actorIndex];
                    
                    if (frequency == EActionFrequency.OncePerActorLife &&
                        !singleFiredObjects.Add(actor.target))
                    {
                        continue;
                    }

                    if (hasProfiler)
                    {
                        handle = profiler.GetHandle(actor.target);
                        listenerName = actor.target.Meta.name;
                        handle.BeginSample(listenerName);
                    }

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

                passedActorContainer.data = _passedActors;

                return result;
            }
        }
    }
}