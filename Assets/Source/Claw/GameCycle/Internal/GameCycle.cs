using System;
using System.Collections.Generic;
using Beast.Claw.Debug;
using Beast.Claw.Profiling;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Beast.Claw.GameCycle
{
    public partial class GameCycle : IDisposable
    {
        private struct UpdateListener
        {
            public object obj;
            public Action<OperationContext> action;
        }
        
        private struct OperationListener
        {
            public object obj;
            public Func<OperationContext, UniTask<OperationResult>> action;
        }

        private readonly Proxy _proxy;
        private readonly GameCycleConfig _config;
        private readonly ProfilerConfig _profilerConfig;
        private readonly DiContainer _container;

        private HashSet<object> _brokenUpdates;
        private Dictionary<object, int> _updateErrorCount;
        private bool _allUpdateIsBroken;

        private Dictionary<string, List<OperationListener>> _operationListeners;
        private Dictionary<string, List<UpdateListener>> _updateListeners;

        private Dictionary<string, HashSet<object>> _updateHashset;
        private Dictionary<string, HashSet<object>> _operationHashset;

        private Dictionary<string, HashSet<object>> _singleFiredObjects;

        // private Dictionary<string, Profiler> _profilers;
        // private bool _profilersInitialized;
        // private Dictionary<string, List<string>> _updateObjectNames;
        // private Dictionary<string, List<string>> _operationObjectNames;
        // private ProfilerTreeStringBuilder _profilerTreeStringBuilder;
        
        private HashSet<object> _registeredListeners;

        private bool _initialized;
        private bool _isSubscribingListeners;
        private Dictionary<string, OperationInfo> _operationEvents;
        private HashSet<string> _updateEvents;

        public GameCycle (
            Proxy proxy, 
            GameCycleConfig config,
            DiContainer container)
        {
            _proxy = proxy;
            _config = config;
            _container = container;

            _proxy.Inject(this);
        }
        
        private void _initialize(GameCycleInitialization input)
        {
            if (_initialized)
            {
                throw new InvalidOperationException("Already initialized");
            }
            
            input.ValidateThrowException(nameof(input));
            
            _initialized = true;

            _operationEvents = new Dictionary<string, OperationInfo>();
            _updateEvents = new HashSet<string>();
            _registeredListeners = new HashSet<object>();
            _singleFiredObjects = new Dictionary<string, HashSet<object>>();
            
            _operationListeners = new Dictionary<string, List<OperationListener>>();
            _updateListeners = new Dictionary<string, List<UpdateListener>>();
            
            HashSet<string> commandIds = new (input.operations.Count + input.updates.Count);
            
            foreach (string id in input.updates)
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new InvalidOperationException("Empty command id in updateEvents list");
                }
                
                if (!commandIds.Add(id))
                {
                    throw new InvalidOperationException($"Command ids must be unique. Duplicated: '{id}'");;
                }
                
                _updateEvents.Add(id);
                _updateListeners[id] = new List<UpdateListener>();
            }
            
            foreach (OperationInfo op in input.operations)
            {
                if (!commandIds.Add(op.id))
                {
                    throw new InvalidOperationException($"Command ids must be unique. Duplicated: '{op.id}'");
                }
                
                if (string.IsNullOrEmpty(op.id))
                {
                    throw new InvalidOperationException("Empty command id in operation list");
                }

                if (op.executedOncePerObject)
                {
                    _singleFiredObjects[op.id] = new HashSet<object>();
                }

                _operationEvents[op.id] = op;
                _operationListeners[op.id] = new List<OperationListener>();
            }
            
            _brokenUpdates = new HashSet<object>();
            _updateErrorCount = new Dictionary<object, int>();

            if (_config.preventDuplicatingSubscription)
            {
                _updateHashset = new Dictionary<string, HashSet<object>>();
                _operationHashset = new Dictionary<string, HashSet<object>>();
                
                foreach (string eventID in _operationEvents.Keys)
                {
                    _operationHashset[eventID] = new HashSet<object>();
                }
                
                foreach (string eventID in _updateEvents)
                {
                    _updateHashset[eventID] = new HashSet<object>();
                }
            }
        }

        private void _registerListeners()
        {
            _isSubscribingListeners = true;
            
            Handle handle = new (this);

            List<IGameCycleListener> listeners = _container.TryResolve<List<IGameCycleListener>>();
            if (listeners == null)
            {
                throw new Exception("Failed to resolve listeners");
            }
            
            foreach (IGameCycleListener subscriber in listeners)
            {
                if (_registeredListeners.Contains(subscriber))
                {
                    continue;
                }

                try
                {
                    handle.currentListener = subscriber;
                    subscriber.Register(handle);
                }
                catch (Exception e)
                {
                    BDebug.LogException(this, e, $"Exception during subscription ({subscriber.GetType()})");

                    throw;
                }
                finally
                {
                    _registeredListeners.Add(subscriber);
                }
            }
            
            _isSubscribingListeners = false;
        }

        // private void _tryInitializeProfiling()
        // {
        //     if (_profilersInitialized)
        //     {
        //         return;
        //     }
        //
        //     _profilerTreeStringBuilder = new ProfilerTreeStringBuilder();
        //     _profilers = new Dictionary<string, Profiler>();
        //     
        //     _operationObjectNames = new Dictionary<string, List<string>>();
        //     foreach (string eventID in _operationEvents)
        //     {
        //         _operationObjectNames[eventID] = _operationListeners[eventID]
        //             .Select(listener => listener.obj.GetType().Name).ToList();
        //         
        //         Profiler profiler = new (_profilerConfig);
        //         profiler.Initialize();
        //         _profilers[eventID] = profiler;
        //     }
        //
        //     _updateObjectNames = new Dictionary<string, List<string>>();
        //     foreach (string eventID in _updateEvents)
        //     {
        //         _updateObjectNames[eventID] = _updateListeners[eventID]
        //             .Select(listener => listener.obj.GetType().Name).ToList();
        //         
        //         Profiler profiler = new (_profilerConfig);
        //         profiler.Initialize();
        //         _profilers[eventID] = profiler;
        //     }
        //     
        //     _proxy.InitializeProfilers();
        //     
        //     _profilersInitialized = true;
        // }

        private async UniTask<OperationResult> _execute<TOwner>(TOwner owner, string eventID) 
            where TOwner : class
        {
            eventID.ValidateNotEmptyOrNullThrowException(nameof(eventID));
            owner.ValidateNotNullThrowException(nameof(owner));
            
            if (_isSubscribingListeners)
            {
                throw new InvalidOperationException("Tried to invoke operation during subscription");
            }

            if (!_operationEvents.TryGetValue(eventID, out OperationInfo operationInfo))
            {
                throw new InvalidOperationException($"Tried to invoke invalid operation '{eventID}'");
            }
            
            OperationResult result = OperationResult.Success();

            HashSet<object> alreadyFired = null;
            if (operationInfo.executedOncePerObject)
            {
                alreadyFired = _singleFiredObjects[eventID];
            }

            // bool profilingActive = _profilerConfig.AnyProfilingActive;
            // Profiler.Handle handle = Profiler.Handle.Disabled;
            // Profiler profiler = null;
            // List<string> objNames = null;
            //
            // if (profilingActive)
            // {
            //     _tryInitializeProfiling();
            //     profiler = _profilers[eventID];
            //     profiler.FetchConfig();
            //     objNames = _operationObjectNames[eventID];
            // }

            List<OperationListener> listeners = _operationListeners[eventID];

            for (int listenerIndex = 0; listenerIndex < listeners.Count; listenerIndex++)
            {
                OperationListener listener = listeners[listenerIndex];

                if (operationInfo.executedOncePerObject && !alreadyFired.Add(listener.obj))
                {
                    continue;
                }
                
                // string sampleName = null;
                
                // if (profilingActive)
                // {
                //     handle = profiler.GetHandle(listener.obj);
                //     sampleName = objNames[listenerIndex];
                //
                //     handle.BeginSample(sampleName);
                // }

                try
                {
                    OperationContext context = default;
                    result = await listener.action.Invoke(context);
                }
                catch (Exception e)
                {
                    BDebug.LogException(owner, e, $"Exception during {eventID} ({listener.obj.GetType()})");
                    break;
                }
                finally
                {
                    // if (profilingActive)
                    // {
                    //     handle.EndSample(sampleName);
                    // }
                }

                if (result.status != EOperationStatus.Success)
                {
                    BDebug.LogError(owner, $"Failed to {eventID} ({listener.obj.GetType()})");
                    break;
                }
            }

            // if (_profilerConfig.enableClawProfiling)
            // {
            //     profiler.RebuildTree();
            //
            //     if (_profilerConfig.logOperationProfiling)
            //     {
            //         string info = _profilerTreeStringBuilder.ToString(profiler.Reader, eventID);
            //     
            //         BDebug.StaticLogInfo(info);
            //     }
            // }

            return result;
        }

        private void _update<TOwner>(TOwner owner, string eventID) where TOwner : class
        {
            eventID.ValidateNotEmptyOrNullThrowException(nameof(eventID));
            owner.ValidateNotNullThrowException(nameof(owner));
            
            if (_allUpdateIsBroken)
            {
                return;
            }
            
            if (_isSubscribingListeners)
            {
                throw new InvalidOperationException("Tried to invoke update during subscription");
            }
            
            if (!_updateEvents.Contains(eventID))
            {
                throw new InvalidOperationException($"Tried to invoke invalid update '{eventID}'");
            }
            
            bool hasBrokenLogic = _brokenUpdates.Count > 0;
            
            // bool profilingActive = _profilerConfig.AnyProfilingActive;
            // Profiler.Handle handle = Profiler.Handle.Disabled;
            // Profiler profiler = null;
            // List<string> objNames = null;
            //
            // if (profilingActive)
            // {
            //     _tryInitializeProfiling();
            //     profiler = _profilers[eventID];
            //     profiler.FetchConfig();
            //     objNames = _updateObjectNames[eventID];
            // }

            List<UpdateListener> updateListeners = _updateListeners[eventID];

            for (int listenerIndex = 0; listenerIndex < updateListeners.Count; listenerIndex++)
            {
                UpdateListener listener = updateListeners[listenerIndex];
                
                if (hasBrokenLogic && _brokenUpdates.Contains(listener.obj))
                {
                    continue;
                }
                
                // string sampleName = null;
                
                // if (profilingActive)
                // {
                //     handle = profiler.GetHandle(listener.obj);
                //     sampleName = objNames[listenerIndex];
                //         
                //     handle.BeginSample(sampleName);
                // }

                try
                {
                    OperationContext context = default;
                    listener.action.Invoke(context);
                }
                catch (Exception e)
                {
                    ProcessUpdatingException(e, eventID, listener.obj, out bool canContinueExecution);

                    if (!canContinueExecution)
                    {
                        _allUpdateIsBroken = true;

                        return;
                    }
                }
                finally
                {
                    // if (profilingActive)
                    // {
                    //     handle.EndSample(sampleName);
                    // }
                }
            }
            
            // if (_profilerConfig.enableClawProfiling)
            // {
            //     profiler.RebuildTree();
            // }
        }

        private void _listenOperation(object owner, string eventID, Func<OperationContext, UniTask<OperationResult>> action)
        {
            eventID.ValidateNotEmptyOrNullThrowException(nameof(eventID));
            owner.ValidateNotNullThrowException(nameof(owner));
            action.ValidateNotNullThrowException(nameof(action));
            
            if (!_operationEvents.ContainsKey(eventID))
            {
                string message = $"'{eventID}' is not valid operation eventID ({owner.GetType()})";
                throw new InvalidOperationException(message);
            }

            if (_config.preventDuplicatingSubscription)
            {
                if (!_operationHashset[eventID].Add(owner))
                {
                    string message = $"'{owner.GetType()}' tried to listen twice to event '{eventID}'";
                    throw new InvalidOperationException(message);
                }
            }
            
            if (_config.preventExternObjectMethodSubscription)
            {
                if (owner != action.Target)
                {
                    string message = $"'{owner.GetType().FullName}' ({eventID}) tried to subscribe a method " +
                                     $"that is implemented not inside '{owner.GetType().FullName}'";
                    throw new InvalidOperationException(message);
                }
            }

            _operationListeners[eventID].Add(new OperationListener
            {
                action = action,
                obj = owner
            });
        }

        private void _listenUpdate(object owner, string eventID, Action<OperationContext> action)
        {
            owner.ValidateNotNullThrowException(nameof(owner));
            eventID.ValidateNotEmptyOrNullThrowException(nameof(eventID));
            action.ValidateNotNullThrowException(nameof(action));
            
            if (!_updateEvents.Contains(eventID))
            {
                string message = $"'{eventID}' is not valid update eventID ({owner.GetType().FullName})";
                throw new InvalidOperationException(message);
            }

            if (_config.preventExternObjectMethodSubscription)
            {
                if (owner != action.Target)
                {
                    string message = $"'{owner.GetType().FullName}' ({eventID}) tried to subscribe a method " +
                                     $"that is implemented not inside '{owner.GetType().FullName}'";
                    throw new InvalidOperationException(message);
                }
            }

            if (_config.preventDuplicatingSubscription)
            {
                if (!_updateHashset[eventID].Add(owner))
                {
                    string message = $"'{owner.GetType().FullName}' tried to listen twice to event '{eventID}'";
                    throw new InvalidOperationException(message);
                }
            }
            
            _updateListeners[eventID].Add(new UpdateListener
            {
                action = action,
                obj = owner
            });
        }
        
        private void ProcessUpdatingException(
            Exception e, 
            string eventName, 
            object logic, 
            out bool canContinueExecution)
        {
            BDebug.LogException(this, e, logic.GetType() + "." + eventName + "()");
            
            _updateErrorCount.TryGetValue(logic, out int errorCount);
            errorCount++;
            _updateErrorCount[logic] = errorCount;
        
            if (errorCount > _config.updateExceptionThreshold)
            {
                _brokenUpdates.Add(logic);
            }
        
            if (_config.exceptionStopsUpdates)
            {
                canContinueExecution = false;
            }
            else
            {
                canContinueExecution = true;
            }
        }

        public void Dispose()
        {
            // if (_profilersInitialized)
            // {
            //     foreach (Profiler profiler in _profilers.Values)
            //     {
            //         profiler.Dispose();
            //     }
            //     
            //     _profilers.Clear();
            //     
            //     _profilersInitialized = false;
            // }
        }
    }
}