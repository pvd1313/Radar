using System;
using System.Collections.Generic;
using Beast.Claw.Debug;
using Beast.Claw.Profiling;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public partial class GameCycle
    {
        private struct RunnerConcurrent
        {
            public HashSet<object> singleFiredObjects;
            public EOperationFrequency frequency;
            public List<Runner.Listener> listeners;
            public string operationID;
            public RunnerProfiler profiler;
            public GameCycleConfig config;
            public ProfilerTreeStringBuilder treeStringBuilder;
            
            public async UniTask<OperationResult> Run()
            {
                OperationResult result = OperationResult.Success();

                if (profiler.isInitialized)
                {
                    profiler.profiler.FetchConfig();
                }
                
                Profiler.Handle handle = default;
                string objectName = null;

                for (int i = 0; i < listeners.Count; i++)
                {
                    Runner.Listener listener = listeners[i];
                    if (frequency == EOperationFrequency.OncePerObjectLife &&
                        !singleFiredObjects.Add(listener.target))
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

                        result = await listener.concurrent.Invoke(context);

                        if (!result.IsSuccess())
                        {
                            BDebug.LogError(this, $"Error during '{operationID}' ({listener.GetType().FullName})");

                            return result;
                        }
                    }
                    catch (Exception e)
                    {
                        BDebug.LogException(this, e,
                            $"Exception during '{operationID}' ({listener.GetType().FullName})");

                        return OperationResult.Failed();
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

                    if (config.flushOperationProfilerToLog)
                    {
                        string info = treeStringBuilder.ToString(profiler.profiler.Reader, operationID);

                        BDebug.StaticLogInfo(info);
                    }
                }

                return result;
            }
        }
    }
}