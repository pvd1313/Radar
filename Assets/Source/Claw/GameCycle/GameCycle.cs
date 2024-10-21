using System;
using System.Collections.Generic;
using Beast.Claw.Debug;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public partial class GameCycle
    {
        private struct UpdateListener
        {
            public object listener;
            public Action action;
        }
        
        private struct InitializationListener
        {
            public object listener;
            public Func<UniTask<OperationResult>> action;
        }
        
        private readonly List<IGameCycleListener> _subscribers;
        private readonly GameCycleConfig _config;
        private readonly GameCycleProxy _proxy;

        private HashSet<object> _brokenUpdates;
        private Dictionary<object, int> _updateErrorCount;
        private bool _allUpdateIsBroken;

        private Dictionary<string, List<InitializationListener>> _initialization;
        private Dictionary<string, List<UpdateListener>> _update;

        private Dictionary<string, HashSet<object>> _updateHashset;
        private Dictionary<string, HashSet<object>> _initializationHashset;

        private bool _isSubscribingListeners;
        private HashSet<string> _initializationEventIDs;
        private HashSet<string> _updateEventIDs;

        public GameCycle (
            GameCycleProxy proxy, 
            GameCycleConfig config, 
            List<IGameCycleListener> subscribers)
        {
            _proxy = proxy;
            _subscribers = subscribers;
            _config = config;

            _proxy.Inject(this);
        }
        
        public bool TryInitializeSelf(GameCycleParameters input)
        {
            input.ValidateThrowException();
            
            _isSubscribingListeners = true;

            _brokenUpdates = new HashSet<object>();
            _updateErrorCount = new Dictionary<object, int>();

            _initializationEventIDs = input.initializationEventIDs;
            _updateEventIDs = input.updateEventIDs;
            
            _initialization = new Dictionary<string, List<InitializationListener>>();
            _update = new Dictionary<string, List<UpdateListener>>();
            foreach (string eventID in _initializationEventIDs)
            {
                _initialization[eventID] = new List<InitializationListener>();
            }
            foreach (string eventID in _updateEventIDs)
            {
                _update[eventID] = new List<UpdateListener>();
            }

            if (_config.preventDuplicatingSubscription)
            {
                _updateHashset = new Dictionary<string, HashSet<object>>();
                _initializationHashset = new Dictionary<string, HashSet<object>>();
                foreach (string eventID in _initializationEventIDs)
                {
                    _initializationHashset[eventID] = new HashSet<object>();
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

        public async UniTask<OperationResult> Execute<TOwner>(TOwner owner, string eventID) 
            where TOwner : class
        {
            eventID.ValidateNotEmptyOrNullThrowException(nameof(eventID));
            owner.ValidateNotNullThrowException(nameof(owner));
            
            if (_isSubscribingListeners)
            {
                throw new InvalidOperationException("Tried to invoke initialization during subscription");
            }

            if (!_initializationEventIDs.Contains(eventID))
            {
                throw new InvalidOperationException($"Tried to invoke invalid initialization event '{eventID}'");
            }
            
            OperationResult result = OperationResult.Success();
            
            foreach (InitializationListener listener in _initialization[eventID])
            {
                try
                {
                    result = await listener.action.Invoke();
                }
                catch (Exception e)
                {
                    BDebug.LogException(owner, e, $"Exception during {eventID} ({listener.listener.GetType()})");
                    break;
                }
                
                if (result.status != EOperationStatus.Success)
                {
                    BDebug.LogError(owner, $"Failed to {eventID} ({listener.listener.GetType()})");
                    break;
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
            
            foreach (UpdateListener listener in _update[eventID])
            {
                if (hasBrokenLogic && _brokenUpdates.Contains(listener.listener))
                {
                    continue;
                }
                
                try
                {
                    listener.action.Invoke();
                }
                catch (Exception e)
                {
                    ProcessUpdatingException(e, eventID, listener.listener, out bool canContinueExecution);

                    if (!canContinueExecution)
                    {
                        _allUpdateIsBroken = true;
                        
                        return;
                    }
                }
            }
        }

        private void _listenOperation(object owner, string eventID, Func<UniTask<OperationResult>> action)
        {
            eventID.ValidateNotEmptyOrNullThrowException(nameof(eventID));
            owner.ValidateNotNullThrowException(nameof(owner));;
            action.ValidateNotNullThrowException(nameof(action));
            
            if (!_initializationEventIDs.Contains(eventID))
            {
                string message = $"'{eventID}' is not valid initialization eventID ({owner.GetType()})";
                throw new InvalidOperationException(message);
            }

            if (_config.preventDuplicatingSubscription)
            {
                if (!_initializationHashset[eventID].Add(owner))
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
            

            _initialization[eventID].Add(new InitializationListener
            {
                action = action,
                listener = owner
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
            
            _update[eventID].Add(new UpdateListener
            {
                action = action,
                listener = owner
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
    }
}