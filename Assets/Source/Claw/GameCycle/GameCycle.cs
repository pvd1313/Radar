using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public class GameCycle
    {
        private struct UpdateListener
        {
            public IGameCycleListener listener;
            public Action action;
        }
        
        private struct InitializationListener
        {
            public IGameCycleListener listener;
            public Func<UniTask<OperationResult>> action;
        }
        
        private readonly List<IGameCycleListener> _subscribers;
        private readonly GameCycleConfig _config;
        private readonly GameCycleProxy _proxy;

        private HashSet<object> _brokenUpdates;
        private Dictionary<object, int> _updateErrorCount;
        private bool _allUpdateIsBroken;
        // private string _currentInitializationEvent;

        private Dictionary<string, List<InitializationListener>> _initialization;
        private Dictionary<string, List<UpdateListener>> _update;

        private Dictionary<string, HashSet<IGameCycleListener>> _updateHashset;
        private Dictionary<string, HashSet<IGameCycleListener>> _initializationHashset;

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
            input.Validate();
            
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
                _updateHashset = new Dictionary<string, HashSet<IGameCycleListener>>();
                _initializationHashset = new Dictionary<string, HashSet<IGameCycleListener>>();
                foreach (string eventID in _initializationEventIDs)
                {
                    _initializationHashset[eventID] = new HashSet<IGameCycleListener>();
                }
                foreach (string eventID in _updateEventIDs)
                {
                    _updateHashset[eventID] = new HashSet<IGameCycleListener>();
                }
            }
            
            foreach (IGameCycleListener subscriber in _subscribers)
            {
                try
                {
                    subscriber.Register(_proxy);
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
            eventID.ValidateNotEmpty(nameof(eventID));
            owner.ValidateNotNull(nameof(owner));
            
            if (_isSubscribingListeners)
            {
                throw new InvalidOperationException("Tried to invoke initialization during subscription");
            }
            
            // if (_currentInitializationEvent != null)
            // {
            //     throw new InvalidOperationException($"Initialization ({_currentInitializationEvent}) " +
            //                                         $"is already in progress. Can not invoke new ({eventID}) ");
            // }

            if (!_initializationEventIDs.Contains(eventID))
            {
                throw new InvalidOperationException($"Tried to invoke invalid initialization event '{eventID}'");
            }
            
            // _currentInitializationEvent = eventID;
            
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
            
            // _currentInitializationEvent = null;
            
            return result;
        }

        public void Update<TOwner>(TOwner owner, string eventID) where TOwner : class
        {
            eventID.ValidateNotEmpty(nameof(eventID));
            owner.ValidateNotNull(nameof(owner));
            
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

        public void ListenOperation<TOwner>(
            TOwner owner, 
            string eventID, 
            Func<UniTask<OperationResult>> action) where TOwner : class, IGameCycleListener
        {
            eventID.ValidateNotEmpty(nameof(eventID));
            owner.ValidateNotNull(nameof(owner));
            action.ValidateNotNull(nameof(action));
            
            
            
            if (!_initializationEventIDs.Contains(eventID))
            {
                throw new InvalidOperationException(
                    $"'{eventID}' is not valid initialization eventID ({typeof(TOwner)})");
            }

            if (_config.preventDuplicatingSubscription)
            {
                if (!_initializationHashset[eventID].Add(owner))
                {
                    throw new InvalidOperationException(
                        $"'{typeof(TOwner)}' tried to listen twice to event '{eventID}'");
                }
            }

            _initialization[eventID].Add(new InitializationListener
            {
                action = action,
                listener = owner
            });
        }

        public void ListenUpdate<TOwner>(
            TOwner owner, 
            string eventID, 
            Action action) where TOwner : class, IGameCycleListener
        {
            owner.ValidateNotNull(nameof(owner));
            eventID.ValidateNotEmpty(nameof(eventID));
            action.ValidateNotNull(nameof(action));
            
            if (!_updateEventIDs.Contains(eventID))
            {
                throw new InvalidOperationException(
                    $"'{eventID}' is not valid update eventID ({typeof(TOwner)})");
            }

            if (_config.preventDuplicatingSubscription)
            {
                if (!_updateHashset[eventID].Add(owner))
                {
                    throw new InvalidOperationException(
                        $"'{typeof(TOwner)}' tried to listen twice to event '{eventID}'");
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