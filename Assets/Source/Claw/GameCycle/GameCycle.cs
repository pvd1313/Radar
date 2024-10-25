using System;
using System.Collections.Generic;
using System.Linq;
using Beast.Claw.Debug;
using Beast.Claw.Profiling;
using Beast.Claw.Reflection;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public partial class GameCycle
    {
        private struct UpdateListener
        {
            public object obj;
            public Action action;
        }
        
        private struct OperationListener
        {
            public object obj;
            public Func<UniTask<OperationResult>> action;
        }

        private readonly Proxy _proxy;
        private readonly List<IGameCycleListener> _subscribers;
        private readonly GameCycleConfig _cycleConfig;
        private readonly ProfilerConfig _profilerConfig;

        private HashSet<object> _brokenUpdates;
        private Dictionary<object, int> _updateErrorCount;
        private bool _allUpdateIsBroken;

        private Dictionary<string, List<OperationListener>> _operationListeners;
        private Dictionary<string, List<UpdateListener>> _updateListeners;

        private Dictionary<string, HashSet<object>> _updateHashset;
        private Dictionary<string, HashSet<object>> _operationHashset;

        private Dictionary<string, Profiler> _profilers;
        private bool _profilersInitialized;
        private Dictionary<string, List<string>> _updateObjectNames;
        private Dictionary<string, List<string>> _operationObjectNames;
        private ProfilerTreeStringBuilder _profilerTreeStringBuilder;

        private bool _isSubscribingListeners;
        private HashSet<string> _operationEventIDs;
        private HashSet<string> _updateEventIDs;

        public GameCycle (
            Proxy proxy, 
            GameCycleConfig cycleConfig, 
            ProfilerConfig profilerConfig,
            List<IGameCycleListener> subscribers)
        {
            _proxy = proxy;
            _subscribers = subscribers;
            _cycleConfig = cycleConfig;
            _profilerConfig = profilerConfig;

            _proxy.Inject(this);
        }
        
        public bool TryInitializeSelf(GameCycleParameters input)
        {
            input.ValidateThrowException(nameof(input));

            List<string> idsIntersection = input.updateEventIDs.Intersect(input.operationEventIDs).ToList();

            if (idsIntersection.Count > 0)
            {
                string conflictingIds = string.Join(", ", idsIntersection);
                throw new InvalidOperationException("Command ids must be unique. Duplicated: " + conflictingIds);
            }
            
            _isSubscribingListeners = true;

            _brokenUpdates = new HashSet<object>();
            _updateErrorCount = new Dictionary<object, int>();

            _operationEventIDs = input.operationEventIDs;
            _updateEventIDs = input.updateEventIDs;
            
            _operationListeners = new Dictionary<string, List<OperationListener>>();
            _updateListeners = new Dictionary<string, List<UpdateListener>>();
            foreach (string eventID in _operationEventIDs)
            {
                _operationListeners[eventID] = new List<OperationListener>();
            }
            foreach (string eventID in _updateEventIDs)
            {
                _updateListeners[eventID] = new List<UpdateListener>();
            }

            if (_cycleConfig.preventDuplicatingSubscription)
            {
                _updateHashset = new Dictionary<string, HashSet<object>>();
                _operationHashset = new Dictionary<string, HashSet<object>>();
                foreach (string eventID in _operationEventIDs)
                {
                    _operationHashset[eventID] = new HashSet<object>();
                }
                foreach (string eventID in _updateEventIDs)
                {
                    _updateHashset[eventID] = new HashSet<object>();
                }
            }

            GameCycleHandle handle = new (this);
            
            foreach (IGameCycleListener subscriber in _subscribers)
            {
                try
                {
                    handle.currentListener = subscriber;
                    subscriber.Register(handle);
                }
                catch (Exception e)
                {
                    BDebug.LogException(this, e, $"Exception during subscription ({subscriber.GetType()})");
                    
                    return false;
                }
            }

            _isSubscribingListeners = false;

            return true;
        }

        private void _tryInitializeProfiling()
        {
            if (_profilersInitialized)
            {
                return;
            }

            _profilerTreeStringBuilder = new ProfilerTreeStringBuilder();
            _profilers = new Dictionary<string, Profiler>();
            
            _operationObjectNames = new Dictionary<string, List<string>>();
            foreach (string eventID in _operationEventIDs)
            {
                _operationObjectNames[eventID] = _operationListeners[eventID]
                    .Select(listener => listener.obj.GetType().Name).ToList();
                
                Profiler profiler = new (_profilerConfig);
                profiler.Initialize();
                _profilers[eventID] = profiler;
            }

            _updateObjectNames = new Dictionary<string, List<string>>();
            foreach (string eventID in _updateEventIDs)
            {
                _updateObjectNames[eventID] = _updateListeners[eventID]
                    .Select(listener => listener.obj.GetType().Name).ToList();
                
                Profiler profiler = new (_profilerConfig);
                profiler.Initialize();
                _profilers[eventID] = profiler;
            }
            
            _proxy.InitializeProfilers();
            
            _profilersInitialized = true;
        }

        public async UniTask<OperationResult> Execute<TOwner>(TOwner owner, string eventID) 
            where TOwner : class
        {
            eventID.ValidateNotEmptyOrNullThrowException(nameof(eventID));
            owner.ValidateNotNullThrowException(nameof(owner));
            
            if (_isSubscribingListeners)
            {
                throw new InvalidOperationException("Tried to invoke operation during subscription");
            }

            if (!_operationEventIDs.Contains(eventID))
            {
                throw new InvalidOperationException($"Tried to invoke invalid operation event '{eventID}'");
            }
            
            OperationResult result = OperationResult.Success();

            bool profilingActive = _profilerConfig.AnyProfilingActive;
            Profiler.Handle handle = Profiler.Handle.Disabled;
            Profiler profiler = null;
            List<string> objNames = null;
            
            if (profilingActive)
            {
                _tryInitializeProfiling();
                profiler = _profilers[eventID];
                profiler.FetchConfig();
                objNames = _operationObjectNames[eventID];
            }

            List<OperationListener> listeners = _operationListeners[eventID];

            for (int listenerIndex = 0; listenerIndex < listeners.Count; listenerIndex++)
            {
                OperationListener listener = listeners[listenerIndex];
                
                string sampleName = null;
                
                if (profilingActive)
                {
                    handle = profiler.GetHandle(listener.obj);
                    sampleName = objNames[listenerIndex];

                    handle.BeginSample(sampleName);
                }

                try
                {

                    result = await listener.action.Invoke();
                }
                catch (Exception e)
                {
                    BDebug.LogException(owner, e, $"Exception during {eventID} ({listener.obj.GetType()})");
                    break;
                }
                finally
                {
                    if (profilingActive)
                    {
                        handle.EndSample(sampleName);
                    }
                }

                if (result.status != EOperationStatus.Success)
                {
                    BDebug.LogError(owner, $"Failed to {eventID} ({listener.obj.GetType()})");
                    break;
                }
            }

            if (_profilerConfig.enableClawProfiling)
            {
                profiler.RebuildTree();

                if (_profilerConfig.logOperationProfiling)
                {
                    string info = _profilerTreeStringBuilder.ToString(profiler.Reader, eventID);
                
                    BDebug.StaticLogInfo(info);
                }
            }

            return result;
        }

        public void Update<TOwner>(TOwner owner, string eventID) where TOwner : class
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
            
            if (!_updateEventIDs.Contains(eventID))
            {
                throw new InvalidOperationException($"Tried to invoke invalid update event '{eventID}'");
            }
            
            bool hasBrokenLogic = _brokenUpdates.Count > 0;
            
            bool profilingActive = _profilerConfig.AnyProfilingActive;
            Profiler.Handle handle = Profiler.Handle.Disabled;
            Profiler profiler = null;
            List<string> objNames = null;
            
            if (profilingActive)
            {
                _tryInitializeProfiling();
                profiler = _profilers[eventID];
                profiler.FetchConfig();
                objNames = _updateObjectNames[eventID];
            }

            List<UpdateListener> updateListeners = _updateListeners[eventID];

            for (int listenerIndex = 0; listenerIndex < updateListeners.Count; listenerIndex++)
            {
                UpdateListener listener = updateListeners[listenerIndex];
                
                if (hasBrokenLogic && _brokenUpdates.Contains(listener.obj))
                {
                    continue;
                }
                
                string sampleName = null;
                
                if (profilingActive)
                {
                    handle = profiler.GetHandle(listener.obj);
                    sampleName = objNames[listenerIndex];
                        
                    handle.BeginSample(sampleName);
                }

                try
                {
                    listener.action.Invoke();
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
                    if (profilingActive)
                    {
                        handle.EndSample(sampleName);
                    }
                }
            }
            
            if (_profilerConfig.enableClawProfiling)
            {
                profiler.RebuildTree();
            }
        }

        private void _listenOperation(object owner, string eventID, Func<UniTask<OperationResult>> action)
        {
            eventID.ValidateNotEmptyOrNullThrowException(nameof(eventID));
            owner.ValidateNotNullThrowException(nameof(owner));;
            action.ValidateNotNullThrowException(nameof(action));
            
            if (!_operationEventIDs.Contains(eventID))
            {
                string message = $"'{eventID}' is not valid operation eventID ({owner.GetType()})";
                throw new InvalidOperationException(message);
            }

            if (_cycleConfig.preventDuplicatingSubscription)
            {
                if (!_operationHashset[eventID].Add(owner))
                {
                    string message = $"'{owner.GetType()}' tried to listen twice to event '{eventID}'";
                    throw new InvalidOperationException(message);
                }
            }
            
            if (_cycleConfig.preventExternObjectMethodSubscription)
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

        private void _listenUpdate(object owner, string eventID, Action action)
        {
            owner.ValidateNotNullThrowException(nameof(owner));
            eventID.ValidateNotEmptyOrNullThrowException(nameof(eventID));
            action.ValidateNotNullThrowException(nameof(action));
            
            if (!_updateEventIDs.Contains(eventID))
            {
                string message = $"'{eventID}' is not valid update eventID ({owner.GetType().FullName})";
                throw new InvalidOperationException(message);
            }

            if (_cycleConfig.preventExternObjectMethodSubscription)
            {
                if (owner != action.Target)
                {
                    string message = $"'{owner.GetType().FullName}' ({eventID}) tried to subscribe a method " +
                                     $"that is implemented not inside '{owner.GetType().FullName}'";
                    throw new InvalidOperationException(message);
                }
            }

            if (_cycleConfig.preventDuplicatingSubscription)
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
        
            if (errorCount > _cycleConfig.updateExceptionThreshold)
            {
                _brokenUpdates.Add(logic);
            }
        
            if (_cycleConfig.exceptionStopsUpdates)
            {
                canContinueExecution = false;
            }
            else
            {
                canContinueExecution = true;
            }
        }
    }
}