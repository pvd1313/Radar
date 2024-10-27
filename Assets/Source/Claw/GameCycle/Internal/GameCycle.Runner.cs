using System;
using System.Collections.Generic;
using Beast.Claw.Collection;
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
                public Action<ActionContext> blocking;
                public Func<ActionContext, UniTask<ActionResult>> concurrent;
                public object target;
            }

            public RunnerProfiler Profiler => _profiler;

            public ActionInfo Operation => _operation;
            
            private readonly GameCycleConfig _config;

            private readonly HashSet<object> _singleFireObjects;
            private readonly HashSet<object> _duplicateSubscriptionListeners;
            private readonly ActionInfo _operation;
            private readonly bool _preventDuplicateSubscription;
            private readonly Dictionary<object, int> _blockingLogicCounters;
            private readonly HashSet<object> _blockingBrokenLogic;

            private ItemBuffer<Listener> _listeners;
            private ItemBuffer<Listener> _lastToFirstTempBuffer;
            private bool _blockingLogicAllBroken;
            private ProfilerTreeStringBuilder _treeBuilder;
            
            private RunnerProfiler _profiler;

            public Runner(GameCycleConfig config, ActionInfo operationInfo)
            {
                config.ValidateNotNullThrowException(nameof(config));
                operationInfo.ValidateThrowException(nameof(operationInfo));
                
                _config = config;
                _operation = operationInfo;
                _preventDuplicateSubscription = _config.preventDuplicateSubscription;

                if (_operation.frequency == EActionFrequency.OncePerObjectLife)
                {
                    _singleFireObjects = new HashSet<object>(_operation.bufferCapacity);
                }

                if (_operation.flow == EActionFlowType.Blocking)
                {
                    _blockingLogicCounters = new Dictionary<object, int>(_operation.bufferCapacity);
                    _blockingBrokenLogic = new HashSet<object>(_operation.bufferCapacity);
                }

                if (operationInfo.order == EActionOrder.FromLastToFirstInjected)
                {
                    _lastToFirstTempBuffer = new ItemBuffer<Listener>(_operation.bufferCapacity);
                }
                
                _listeners = new ItemBuffer<Listener>(_operation.bufferCapacity);

                if (_preventDuplicateSubscription)
                {
                    _duplicateSubscriptionListeners = new HashSet<object>(_operation.bufferCapacity);
                }
            }

            public void AddListener(Listener listener)
            {
                listener.target.ValidateNotNullThrowException("listener.target");

                if (_preventDuplicateSubscription && !_duplicateSubscriptionListeners.Add(listener.target))
                {
                    string message =
                        $"'{listener.target.GetType().FullName}' is already listening to '{_operation.id}'";
                    throw new InvalidOperationException(message);
                }

                switch (_operation.flow)
                {
                    case EActionFlowType.Blocking:
                        listener.blocking.ValidateNotNullThrowException("listener.blocking");
                        break;
                        
                    case EActionFlowType.Concurrent:
                        listener.concurrent.ValidateNotNullThrowException("listener.concurrent");
                        break;
                }

                if (_operation.order == EActionOrder.FromFirstToLastInjected)
                {
                    if (_listeners.Count >= _operation.bufferCapacity)
                    {
                        throw new OutOfMemoryException("Not enough capacity to store all listeners");
                    }
                    
                    _listeners.Add(listener);
                }
                else
                {
                    if (_listeners.Count + _lastToFirstTempBuffer.Count >= _operation.bufferCapacity)
                    {
                        throw new OutOfMemoryException("Not enough capacity to store all listeners");
                    }

                    _lastToFirstTempBuffer.Add(listener);
                }

                if (_profiler.IsInitialized)
                {
                    _profiler.Dispose();
                    _profiler = default;
                }
            }

            public async UniTask<ActionResult> ExecuteConcurrent<TOwner>(TOwner owner)
                where TOwner : class
            {
                owner.ValidateNotNullThrowException(nameof(owner));
                
                if (_operation.flow != EActionFlowType.Concurrent)
                {
                    throw new InvalidOperationException("Invalid flow type");
                }

                _maintainListenersList();

                ActionResult result = await new RunnerConcurrent
                {
                    owner = owner,
                    profiler = _setupProfiler(),
                    treeStringBuilder = _treeBuilder,
                    singleFiredObjects = _singleFireObjects,
                    operationID = _operation.id,
                    listeners = _listeners,
                    frequency = _operation.frequency,
                    flushProfilerToConsole = _config.logConcurrentAction,
                    enableClawProfiling = _config.profiler.enableClawProfiling
                }.Run();

                return result;
            }

            public void ExecuteBlocking()
            {
                if (_operation.flow != EActionFlowType.Blocking)
                {
                    throw new InvalidOperationException("Invalid flow type");
                }

                if (_blockingLogicAllBroken)
                {
                    return;
                }

                _maintainListenersList();

                RunnerBlocking runner = new()
                {
                    profiler = _setupProfiler(),
                    listeners = _listeners,
                    exceptionStopsExecution = _config.exceptionStopsUpdates,
                    exceptionThreshold = _config.updateExceptionThreshold,
                    operationID = _operation.id,
                    brokenObjects = _blockingBrokenLogic,
                    objectErrorCount = _blockingLogicCounters,
                    enableClawProfiling = _config.profiler.enableClawProfiling
                };
                
                runner.Run();

                _blockingLogicAllBroken = runner.allLogicIsBroken;
            }

            private void _maintainListenersList()
            {
                if (_operation.order == EActionOrder.FromLastToFirstInjected && 
                    _lastToFirstTempBuffer.Count > 0)
                {
                    _listeners.InsertItemsAtZeroInReverseOrder(_lastToFirstTempBuffer);
                    _lastToFirstTempBuffer.Clear();
                }
            }

            private RunnerProfiler _setupProfiler()
            {
                if (_config.profiler.AnyProfilingActive)
                {
                    if (_operation.flow == EActionFlowType.Concurrent)
                    {
                        _treeBuilder ??= new ProfilerTreeStringBuilder();
                    }
                    
                    if (!_profiler.IsInitialized)
                    {
                        _profiler = new RunnerProfiler(_config.profiler, _listeners);
                    }

                    return _profiler;
                }

                return default;
            }
            
            public void Dispose()
            {
                if (_lastToFirstTempBuffer.IsInitialized)
                {
                    _lastToFirstTempBuffer = default;
                }

                if (_listeners.IsInitialized)
                {
                    _listeners = default;
                }
                
                if (_profiler.IsInitialized)
                {
                    _profiler.Dispose();
                    _profiler = default;
                }
            }
        }
    }
}