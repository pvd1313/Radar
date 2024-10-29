using System;
using System.Collections.Generic;
using Beast.Claw.Collection;
using Beast.Claw.Debug;
using Beast.Claw.Profiling;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public partial class GameCycle
    {
        private struct RunnerConcurrent
        {
            public object owner;
            public HashSet<object> singleFiredObjects;
            public EActionFrequency frequency;
            public ItemBuffer<Runner.Listener> listeners;
            public string operationID;
            public RunnerProfiler profiler;
            public ProfilerTreeStringBuilder treeStringBuilder;
            public bool enableClawProfiling;
            public bool flushProfilerToConsole;
            
            public async UniTask<ActionResult> Run()
            {
                ActionResult result = ActionResult.Success();

                if (profiler.IsInitialized)
                {
                    profiler.Profiler.FetchConfig();
                }
                
                Profiler.Handle handle = default;
                string objectName = null;

                for (int i = 0; i < listeners.Count; i++)
                {
                    Runner.Listener listener = listeners[i];
                    if (frequency == EActionFrequency.OncePerObjectLife &&
                        !singleFiredObjects.Add(listener.target))
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
                        ConcurrentContext context = default;

                        result = await listener.concurrent.Invoke(context);

                        if (!result.IsSuccess())
                        {
                            BDebug.LogError(this, $"Error during '{operationID}' ({listener.GetType().FullName})");

                            return result;
                        }
                    }
                    catch (Exception e)
                    {
                        BDebug.LogException(e);

                        return ActionResult.Failed();
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

                    if (flushProfilerToConsole)
                    {
                        string title = owner.GetType().Name + ": " + operationID;
                        string info = treeStringBuilder.ToString(profiler.Profiler.Reader, title);

                        BDebug.StaticLogInfo(info);
                    }
                }

                return result;
            }
        }
    }
}