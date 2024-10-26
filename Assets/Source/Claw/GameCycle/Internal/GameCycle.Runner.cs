using System;
using System.Collections.Generic;
using Beast.Claw.Profiling;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public partial class GameCycle
    {
        private class Runner
        {
            public struct Listener
            {
                public Action<OperationContext> blocking;
                public Func<OperationContext, UniTask<OperationResult>> concurrent;
                public object target;
            }

            public RunnerProfiler Profiler => _profiler;

            public OperationInfo Operation => _operation;
            
            private readonly GameCycleConfig _config;

            private readonly HashSet<object> _singleFireObjects;
            private readonly List<Listener> _listeners;
            private readonly HashSet<object> _listenerHashset;
            private readonly OperationInfo _operation;

            private readonly Dictionary<object, int> _blockingLogicCounters;
            private readonly HashSet<object> _blockingBrokenLogic;
            private bool _blockingLogicAllBroken;
            private ProfilerTreeStringBuilder _treeBuilder;
            
            private RunnerProfiler _profiler;

            public Runner(GameCycleConfig config, OperationInfo operationInfo)
            {
                config.ValidateNotNullThrowException(nameof(config));
                operationInfo.ValidateThrowException(nameof(operationInfo));
                
                _config = config;
                _operation = operationInfo;

                if (operationInfo.frequency == EOperationFrequency.OncePerObjectLife)
                {
                    _singleFireObjects = new HashSet<object>();
                }

                if (operationInfo.flow == EOperationFlowType.Blocking)
                {
                    _blockingLogicCounters = new Dictionary<object, int>();
                    _blockingBrokenLogic = new HashSet<object>();
                }
                
                _listeners = new List<Listener>();
                _listenerHashset = new HashSet<object>();
            }

            public void AddListener(Listener listener)
            {
                listener.target.ValidateNotNullThrowException("listener.target");

                if (!_listenerHashset.Add(listener.target))
                {
                    string message =
                        $"'{listener.target.GetType().FullName}' is already listening to '{_operation.id}'";
                    throw new InvalidOperationException(message);
                }

                switch (_operation.flow)
                {
                    case EOperationFlowType.Blocking:
                        listener.blocking.ValidateNotNullThrowException("listener.blocking");
                        break;
                        
                    case EOperationFlowType.Concurrent:
                        listener.concurrent.ValidateNotNullThrowException("listener.concurrent");
                        break;
                }

                if (_operation.order == EOperationOrder.FromFirstToLastInjected)
                {
                    // https://risenbeast.youtrack.cloud/issue/R-79/Game-loading-optimization
                    // TODO: also this might create spikes. It is better to increase capacity ealier.
                    _listeners.Add(listener);
                }
                else
                {
                    // TODO: This is not effective
                    _listeners.Insert(0, listener);
                }

                if (_profiler.isInitialized)
                {
                    _profiler.Dispose();
                    _profiler = default;
                }
            }

            public async UniTask<OperationResult> ExecuteConcurrent()
            {
                if (_operation.flow != EOperationFlowType.Concurrent)
                {
                    throw new InvalidOperationException("Invalid flow type");
                }

                RunnerProfiler profiler = default;

                if (_config.profiler.AnyProfilingActive)
                {
                    _treeBuilder ??= new ProfilerTreeStringBuilder();

                    if (!_profiler.isInitialized)
                    {
                        _profiler = new RunnerProfiler(_config.profiler, _listeners);
                    }
                    
                    profiler = _profiler;
                }

                OperationResult result = await new RunnerConcurrent
                {
                    profiler = profiler,
                    treeStringBuilder = _treeBuilder,
                    singleFiredObjects = _singleFireObjects,
                    operationID = _operation.id,
                    listeners = _listeners,
                    frequency = _operation.frequency, 
                    config = _config
                }.Run();

                return result;
            }

            public void ExecuteBlocking()
            {
                if (_operation.flow != EOperationFlowType.Blocking)
                {
                    throw new InvalidOperationException("Invalid flow type");
                }

                if (_blockingLogicAllBroken)
                {
                    return;
                }
                
                RunnerProfiler profiler = default;

                if (_config.profiler.AnyProfilingActive)
                {
                    if (!_profiler.isInitialized)
                    {
                        _profiler = new RunnerProfiler(_config.profiler, _listeners);
                    }
                    
                    profiler = _profiler;
                }

                RunnerBlocking runner = new()
                {
                    profiler = profiler,
                    listeners = _listeners,
                    exceptionStopsExecution = _config.exceptionStopsUpdates,
                    exceptionThreshold = _config.updateExceptionThreshold,
                    operationID = _operation.id,
                    brokenObjects = _blockingBrokenLogic,
                    objectErrorCount = _blockingLogicCounters,
                    config = _config
                };
                
                runner.Run();

                _blockingLogicAllBroken = runner.allLogicIsBroken;
            }

            public void Dispose()
            {
                if (_profiler.isInitialized)
                {
                    _profiler.Dispose();
                    _profiler = default;
                }
            }
        }
    }
}