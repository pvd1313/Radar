using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Beast.Claw.Collection;
using Beast.Claw.Profiling;

namespace Beast.Claw.GameCycle
{
    public partial class GameCycle
    {
        private struct RunnerProfiler : IDisposable
        {
            public bool IsInitialized => _isInitialized;
            public Profiler Profiler => _profiler;
            public ItemBuffer<string> ObjectNames => _objectNames;
            
            private bool _isInitialized;
            private Profiler _profiler;
            private ItemBuffer<string> _objectNames;
            
            public RunnerProfiler(ProfilerConfig config, ItemBuffer<Runner.Listener> listeners)
            {
                _objectNames = new ItemBuffer<string>(listeners.Capacity);
                
                for (int i = 0; i < listeners.Count; i++)
                {
                    _objectNames.Add(listeners[i].target.GetType().Name);
                }

                _profiler = new Profiler(config);
                _profiler.Initialize();
                _isInitialized = true;
            }

            public void Dispose()
            {
                _profiler?.Dispose();

                if (_objectNames.IsInitialized)
                {
                    _objectNames = default;
                }

                _isInitialized = false;
            }
        }
    }
}