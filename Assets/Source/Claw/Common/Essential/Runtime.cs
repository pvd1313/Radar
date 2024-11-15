using System;
using System.Diagnostics;
using Beast.Claw.GameCycle;
using Beast.Claw.Input;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.Common
{
    public class Runtime : Controller, IGameCycleListener, IRuntime
    {
        private const float SIMULATOR_FETCH_RATE_SEC = 1;
        
        public bool IsMobileSimulatorActive { get; private set; }
        
        private UnityEditorSimulatorWindow _simulatorWindow;

        private DateTime _simulatorWindowLastFetchTime;

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
            handle.ListenBlocking(ActionID.TICK, _tick);
        }
        
        private UniTask _warmup(ConcurrentContext context)
        {
            _simulatorWindow = new UnityEditorSimulatorWindow();
            
            return UniTask.CompletedTask;
        }

        private void _tick(BlockingContext obj)
        {
            _simulatorWindowFetch();
        }

        [Conditional("DEVELOPMENT")]
        private void _simulatorWindowFetch()
        {
            if (!_simulatorWindow.CanFocus)
            {
                return;
            }
                
            DateTime now = DateTime.UtcNow;

            if (now.GetTimePassedFrom(_simulatorWindowLastFetchTime).TotalSeconds > SIMULATOR_FETCH_RATE_SEC)
            {
                _simulatorWindowLastFetchTime = now;

                IsMobileSimulatorActive = _simulatorWindow.CheckHasFocus();
            }
        }
    }
}