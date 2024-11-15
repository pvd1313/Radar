using System;
using System.Collections.Generic;
using Beast.Claw.Collection;
using Beast.Claw.Common;
using Beast.Claw.Profiling;
using Cysharp.Threading.Tasks;
using Unity.Collections;

namespace Beast.Claw.GameLoop
{
    public partial class GameLoop
    {
        private class Runner
        {
            public struct Actor
            {
                public Action<BlockingContext> blocking;
                public Func<ConcurrentContext, UniTask> concurrent;
                public IController target;
            }

            public Profiler Profiler => _profiler;

            public ActionProperties Operation => _operation;
            
            private readonly GameLoopConfig _config;
            private readonly ActionProperties _operation;

            private HashSet<object> _singleFireActors;
            private HashSet<object> _duplicateSubscriptionActors;
            private NativeHashMap<int, int> _errorCounters;
            private ManagedFixedList<Actor> _allActors;
            private ManagedFixedList<Actor> _lastToFirstBuffer;
            private NativeFixedList<short> _passedActorBuffer;
            private ProfilerTreeStringBuilder _treeBuilder;
            private Profiler _profiler;

            public Runner(GameLoopConfig config, ActionProperties operationProperties)
            {
                config.ValidateNotNullThrowException(nameof(config));
                operationProperties.ValidateThrowException(nameof(operationProperties));
                
                _config = config;
                _operation = operationProperties;

                if (_operation.frequency == EActionFrequency.OncePerActorLife)
                {
                    _singleFireActors = new HashSet<object>(_operation.bufferCapacity);
                }

                if (_operation.flow == EActionFlowType.Blocking)
                {
                    _errorCounters = new NativeHashMap<int, int>(_operation.bufferCapacity, Allocator.Persistent);
                }

                if (operationProperties.order == EActionOrder.FromLastToFirstBinded)
                {
                    _lastToFirstBuffer = new ManagedFixedList<Actor>(_operation.bufferCapacity);
                }
                
                _allActors = new ManagedFixedList<Actor>(_operation.bufferCapacity);
                _passedActorBuffer = new NativeFixedList<short>(_operation.bufferCapacity, Allocator.Persistent);

                if (Build.IS_DEVELOPMENT)
                {
                    _duplicateSubscriptionActors = new HashSet<object>(_operation.bufferCapacity);
                }
            }

            public void AddActor(Actor actor)
            {
                actor.target.ValidateNotNullThrowException("actor.target");

                if (Build.IS_DEVELOPMENT)
                {
                    if (!_duplicateSubscriptionActors.Add(actor.target))
                    {
                        throw new InvalidOperationException($"'{actor.target.Meta.name}' " +
                                                            $"is already listening to '{_operation.id}'");
                    }
                }

                switch (_operation.flow)
                {
                    case EActionFlowType.Blocking:
                        actor.blocking.ValidateNotNullThrowException("actor.blocking");
                        break;
                        
                    case EActionFlowType.Concurrent:
                        actor.concurrent.ValidateNotNullThrowException("actor.concurrent");
                        break;
                }

                if (_operation.order == EActionOrder.FromFirstToLastBinded)
                {
                    if (Build.IS_DEVELOPMENT)
                    {
                        if (_allActors.Count >= _operation.bufferCapacity)
                        {
                            throw new OutOfMemoryException("Not enough capacity to store all actors");
                        }
                    }
                    
                    _allActors.Add(actor);
                }
                else
                {
                    if (Build.IS_DEVELOPMENT)
                    {
                        if (_allActors.Count + _lastToFirstBuffer.Count >= _operation.bufferCapacity)
                        {
                            throw new OutOfMemoryException("Not enough capacity to store all actors");
                        }
                    }

                    _lastToFirstBuffer.Add(actor);
                }
            }

            public async UniTask<EActionStatus> ExecuteConcurrent(IController owner)
            {
                owner.ValidateNotNullThrowException(nameof(owner));
                
                if (Build.IS_DEVELOPMENT)
                {
                    if (_operation.flow != EActionFlowType.Concurrent)
                    {
                        throw new InvalidOperationException("Invalid flow type");
                    }
                }

                _maintainActorMainList();

                RunnerConcurrent runner = new ()
                {
                    owner = owner,
                    profiler = _setupProfiler(),
                    treeStringBuilder = _treeBuilder,
                    singleFiredObjects = _singleFireActors,
                    operationID = _operation.id,
                    allActors = _allActors,
                    frequency = _operation.frequency,
                    flushProfilerToConsole = _operation.flushProfilerToConsole,
                    enableClawProfiling = _config.profiler.customProfilingEnabled,
                    passedActors = _passedActorBuffer
                };

                EActionStatus result = await runner.Run();

                _passedActorBuffer = runner.passedActors;
                _checkoutPassedActors();

                return result;
            }

            public void ExecuteBlocking(IController owner)
            {
                if (Build.IS_DEVELOPMENT)
                {
                    if (_operation.flow != EActionFlowType.Blocking)
                    {
                        throw new InvalidOperationException("Invalid flow type");
                    }
                }

                _maintainActorMainList();

                RunnerBlocking runner = new()
                {
                    profiler = _setupProfiler(),
                    allActors = _allActors,
                    exceptionStopsExecution = _config.exceptionStopsUpdates,
                    exceptionThreshold = _config.updateExceptionThreshold,
                    operationID = _operation.id,
                    actorErrorCounters = _errorCounters,
                    enableClawProfiling = _config.profiler.customProfilingEnabled,
                    flushProfilerToConsole = _operation.flushProfilerToConsole, 
                    treeStringBuilder = _treeBuilder,
                    passedActors = _passedActorBuffer,
                    owner = owner,
                };
                
                runner.Run();

                _passedActorBuffer = runner.passedActors;
                _checkoutPassedActors();
            }
            
            private void _checkoutPassedActors()
            {
                if (_allActors.Count == _passedActorBuffer.Count)
                {
                    return;
                }

                ManagedFixedList<Actor> newBuffer = new(_operation.bufferCapacity);

                for (int i = 0; i < _passedActorBuffer.Count; i++)
                {
                    int oldIndex = _passedActorBuffer[i];
                    
                    newBuffer.Add(_allActors[oldIndex]);
                }

                _allActors = newBuffer;
                
                _passedActorBuffer.ResetCount();
            }

            private void _maintainActorMainList()
            {
                if (_operation.order == EActionOrder.FromLastToFirstBinded && 
                    _lastToFirstBuffer.Count > 0)
                {
                    _allActors.InsertItemsAtZeroInReverseOrder(_lastToFirstBuffer);
                    _lastToFirstBuffer.ResetCount();
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
                _lastToFirstBuffer = default;
                _allActors = default;

                _passedActorBuffer.SafeDispose();

                if (_errorCounters.IsCreated)
                {
                    _errorCounters.Dispose();
                }
                
                _profiler?.Dispose();
                _profiler = null;

                _singleFireActors = null;
                _duplicateSubscriptionActors = null;
            }
        }
    }
}