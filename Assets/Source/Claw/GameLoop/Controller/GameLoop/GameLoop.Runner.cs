using System;
using System.Collections.Generic;
using Beast.Claw.Collections;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.Profiling;
using Beast.Claw.ProgressTracker;
using Beast.Claw.Validation;
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

            public BricksProfiler Profiler => _profiler;

            public ActionProperties Operation => _operation;
            
            private readonly GameLoopConfig _config;
            private readonly ActionProperties _operation;
            private readonly Container<NativeFixedList<short>> _activeActorContainer;

            private HashSet<object> _singleFireActors;
            private HashSet<object> _duplicateSubscriptionActors;
            private NativeHashMap<int, int> _errorCounters;
            private ManagedFixedList<Actor> _allActors;
            private ManagedFixedList<Actor> _lastToFirstBuffer;
            private NativeFixedList<short> _activeActorsBuffer;
            private BricksTreeStringBuilder _treeBuilder;
            private BricksProfiler _profiler;

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
                _activeActorsBuffer = new NativeFixedList<short>(_operation.bufferCapacity, Allocator.Persistent);
                _activeActorContainer = new Container<NativeFixedList<short>>();

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
                        throw new InvalidOperationException($"'{actor.target.Meta.fullName}' " +
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

            public async UniTask<EActionStatus> ExecuteConcurrent(IController owner, IProgressTracker progressTracker)
            {
                owner.ValidateNotNullThrowException(nameof(owner));
                
                if (Build.IS_DEVELOPMENT)
                {
                    if (_operation.flow != EActionFlowType.Concurrent)
                    {
                        throw new InvalidOperationException("Invalid flow type");
                    }
                }

                _fetchActorList();

                if (progressTracker != null)
                {
                    int actorCount = _allActors.Count;
                    
                    if (_operation.frequency == EActionFrequency.OncePerActorLife)
                    {
                        actorCount -= _singleFireActors.Count;
                    }

                    progressTracker.Begin(actorCount, 1);
                }
                
                RunnerConcurrent runner = new()
                {
                    profiler = _setupProfiler(owner),
                    progressTracker = progressTracker,
                    singleFiredObjects = _singleFireActors,
                    allActors = _allActors,
                    frequency = _operation.frequency,
                    activeActorsContainer = _activeActorContainer.SetData(_activeActorsBuffer),
                };

                EActionStatus result = await runner.Run();

                _rebuildProfilerTree(owner);

                _activeActorsBuffer = _activeActorContainer.data;
                _removeDisabledActors();

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

                _fetchActorList();

                RunnerBlocking runner = new()
                {
                    profiler = _setupProfiler(owner),
                    allActors = _allActors,
                    exceptionStopsExecution = _config.exceptionStopsUpdates,
                    exceptionThreshold = _config.updateExceptionThreshold,
                    actorErrorCounters = _errorCounters,
                    activeActors = _activeActorsBuffer,
                };
                
                runner.Run();

                _rebuildProfilerTree(owner);

                _activeActorsBuffer = runner.activeActors;
                _removeDisabledActors();
            }
            
            private void _removeDisabledActors()
            {
                if (_allActors.Count == _activeActorsBuffer.Count)
                {
                    return;
                }

                ManagedFixedList<Actor> newBuffer = new(_operation.bufferCapacity);

                for (int i = 0; i < _activeActorsBuffer.Count; i++)
                {
                    int oldIndex = _activeActorsBuffer[i];
                    
                    newBuffer.Add(_allActors[oldIndex]);
                }

                _allActors = newBuffer;
                
                _activeActorsBuffer.Clear();
            }

            private void _fetchActorList()
            {
                if (_operation.order == EActionOrder.FromLastToFirstBinded && 
                    _lastToFirstBuffer.Count > 0)
                {
                    _allActors.InsertItemsAtZeroInReverseOrder(_lastToFirstBuffer);
                    _lastToFirstBuffer.ResetCount();
                }
            }

            private BricksProfiler.Handle _setupProfiler(IController owner)
            {
                if (_config.bricks.bricksEnabled || 
                    BProfiler.UnityProfilerActive)
                {
                    if (_operation.flushProfilerToConsole)
                    {
                        _treeBuilder ??= new BricksTreeStringBuilder();
                    }

                    if (_profiler == null)
                    {
                        _profiler = new BricksProfiler(_config.bricks);
                        _profiler.Initialize();
                    }

                    _profiler.SetActive(_config.bricks.bricksEnabled, BProfiler.UnityProfilerActive);

                    return _profiler.GetHandle(owner);
                }

                return BricksProfiler.Handle.Disabled;
            }

            private void _rebuildProfilerTree(IController owner)
            {
                if (_config.bricks.bricksEnabled)
                {
                    _profiler.RebuildTree();

                    if (_operation.flushProfilerToConsole)
                    {
                        string title = owner.Meta.fullName + ": " + _operation.id;
                        string info = _treeBuilder.ToString(_profiler.Reader, title);
                        
                        BDebug.StaticLogInfo(info);
                    }
                }
            }
            
            public void Dispose()
            {
                _lastToFirstBuffer = default;
                _allActors = default;

                _activeActorsBuffer.Dispose();

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