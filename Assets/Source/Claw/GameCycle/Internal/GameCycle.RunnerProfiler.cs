using System;
using System.Collections.Generic;
using Beast.Claw.Profiling;

namespace Beast.Claw.GameCycle
{
    public partial class GameCycle
    {
        private struct RunnerProfiler : IDisposable
        {
            public readonly bool isInitialized;
            public readonly List<string> objectNames;
            public readonly Profiler profiler;
            
            public RunnerProfiler(ProfilerConfig config, List<Runner.Listener> listeners)
            {
                objectNames = new List<string>(listeners.Count);

                foreach (Runner.Listener listener in listeners)
                {
                    objectNames.Add(listener.target.GetType().Name);
                }

                profiler = new Profiler(config);
                profiler.Initialize();
                isInitialized = true;
            }

            public void Dispose()
            {
                profiler.Dispose();
            }
        }
    }
}