using System;
using System.Collections.Generic;
using Beast.Claw.Collection;
using Beast.Claw.Common;
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
                public Action<BlockingContext> blocking;
                public Func<ConcurrentContext, UniTask> concurrent;
                public IController target;
            }

            public Profiler Profiler => _profiler;

            public ActionInfo Operation => _operation;
            
            private readonly GameCycleConfig _config;

            private readonly HashSet<object> _singleFireObjects;
            private readonly HashSet<object> _duplicateSubscriptionListeners;
            private readonly ActionInfo _operation;
            private readonly Dictionary<object, int> _blockingLogicCounters;
            private readonly HashSet<object> _blockingBrokenLogic;

            private ItemBuffer<Listener> _listeners;
            private ItemBuffer<Listener> _lastToFirstTempBuffer;
            private bool _blockingLogicAllBroken;
            private ProfilerTreeStringBuilder _treeBuilder;
            private Profiler _profiler;

            public Runner(GameCycleConfig config, ActionInfo operationInfo)
            {
                config.ValidateNotNullThrowException(nameof(config));
                operationInfo.ValidateThrowException(nameof(operationInfo));
                
                _config = config;
                _operation = operationInfo;

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

                if (Build.IS_DEVELOPMENT)
                {
                    _duplicateSubscriptionListeners = new HashSet<object>(_operation.bufferCapacity);
                }
            }

            public void AddListener(Listener listener)
            {
                listener.target.ValidateNotNullThrowException("listener.target");

                if (Build.IS_DEVELOPMENT && !_duplicateSubscriptionListeners.Add(listener.target))
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
                    if (Build.IS_DEVELOPMENT && _listeners.Count >= _operation.bufferCapacity)
                    {
                        throw new OutOfMemoryException("Not enough capacity to store all listeners");
                    }
                    
                    _listeners.Add(listener);
                }
                else
                {
                    if (Build.IS_DEVELOPMENT && _listeners.Count + _lastToFirstTempBuffer.Count >= _operation.bufferCapacity)
                    {
                        throw new OutOfMemoryException("Not enough capacity to store all listeners");
                    }

                    _lastToFirstTempBuffer.Add(listener);
                }
            }

            public async UniTask<ActionResult> ExecuteConcurrent(IController owner)
            {
                owner.ValidateNotNullThrowException(nameof(owner));
                
                if (Build.IS_DEVELOPMENT && _operation.flow != EActionFlowType.Concurrent)
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
                    flushProfilerToConsole = _operation.flushProfilerToConsole,
                    enableClawProfiling = _config.profiler.customProfilingEnabled
                }.Run();

                return result;
            }

            public void ExecuteBlocking(IController owner)
            {
                if (Build.IS_DEVELOPMENT && _operation.flow != EActionFlowType.Blocking)
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
                    enableClawProfiling = _config.profiler.customProfilingEnabled,
                    flushProfilerToConsole = _operation.flushProfilerToConsole, 
                    treeStringBuilder = _treeBuilder,
                    owner = owner
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

            private Profiler _setupProfiler()
            {
                if (_config.profiler.AnyProfilingActive)
                {
                    if (_operation.flushProfilerToConsole)
                    {
                        _treeBuilder ??= new ProfilerTreeStringBuilder();
                    }

                    if (_profiler == null)
                    {
                        _profiler = new Profiler(_config.profiler);
                        _profiler.Initialize();
                    }
                    
                    _profiler.FetchConfig();

                    return _profiler;
                }

                return null;
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
                
                _profiler?.Dispose();
                _profiler = null;
            }
        }
    }
}