using System;
using System.Collections.Generic;
using Beast.Claw.Collection;
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
            public ManagedFixedList<Runner.Listener> listeners;
            public string operationID;
            public Profiler profiler;
            public ProfilerTreeStringBuilder treeStringBuilder;
            public bool enableClawProfiling;
            public bool flushProfilerToConsole;
            
            public async UniTask<ActionResult> Run()
            {
                ActionResult result = ActionResult.Success();

                bool hasProfiler = profiler != null;
                
                Profiler.Handle handle = default;
                string listenerName = null;

                for (int i = 0; i < listeners.Count; i++)
                {
                    Runner.Listener listener = listeners[i];
                    if (frequency == EActionFrequency.OncePerObjectLife &&
                        !singleFiredObjects.Add(listener.target))
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
                        ConcurrentContext context = default;

                        await listener.concurrent.Invoke(context);
                    }
                    catch (Exception e)
                    {
                        BDebug.LogException(e);

                        return ActionResult.Failed();
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

                return result;
            }
        }
    }
}