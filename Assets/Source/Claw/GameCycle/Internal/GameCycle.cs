using System;
using System.Collections.Generic;
using Beast.Claw.Debug;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Beast.Claw.GameCycle
{
    public partial class GameCycle
    {
        private readonly GameCycleConfig _config;
        private readonly DiContainer _container;

        private Dictionary<string, Runner> _runners;
        private List<string> _operationIds;

        private bool _initialized;
        private bool _isRegisteringListeners;
        private int _listenerCount;

        public GameCycle (Proxy proxy, GameCycleConfig config, DiContainer container)
        {
            _config = config;
            _container = container;

            proxy.Inject(this);
        }
        
        private void _initialize(GameCycleInitialization input)
        {
            if (_initialized)
            {
                throw new InvalidOperationException("Already initialized");
            }
            
            input.ValidateThrowException(nameof(input));
            
            _initialized = true;

            _runners = new Dictionary<string, Runner>();
            _operationIds = new List<string>(input.actions.Count);

            foreach (ActionInfo operation in input.actions)
            {
                _runners[operation.id] = new Runner(_config, operation);
                _operationIds.Add(operation.id);
            }
        }

        private void _registerListeners()
        {
            _isRegisteringListeners = true;
            
            Handle handle = new (this);
            
            List<IGameCycleListener> listeners = _container.TryResolve<List<IGameCycleListener>>();
            
            if (listeners == null)
            {
                throw new Exception("Failed to resolve listeners");
            }

            if (listeners.Count < _listenerCount)
            {
                throw new Exception("New listener count is less than already registered listener count");
            }

            for (int i = _listenerCount; i < listeners.Count; i++)
            {
                IGameCycleListener subscriber = listeners[i];
                
                try
                {
                    handle.currentListener = subscriber;
                    subscriber.Register(handle);
                }
                catch (Exception e)
                {
                    BDebug.LogException(this, e, $"Exception during registration ({subscriber.GetType()})");

                    throw;
                }
            }

            _listenerCount = listeners.Count;

            _isRegisteringListeners = false;
        }

        private async UniTask<ActionResult> _execute<TOwner>(TOwner owner, string eventID) 
            where TOwner : class
        {
            eventID.ValidateNotEmptyOrNullThrowException(nameof(eventID));
            owner.ValidateNotNullThrowException(nameof(owner));
            
            if (_isRegisteringListeners)
            {
                throw new InvalidOperationException("Tried to invoke operation during registration");
            }

            if (!_runners.TryGetValue(eventID, out Runner runner))
            {
                throw new InvalidOperationException($"Tried to invoke invalid operation '{eventID}'");
            }

            return await runner.ExecuteConcurrent(owner);
        }

        private void _update<TOwner>(TOwner owner, string eventID) where TOwner : class
        {
            eventID.ValidateNotEmptyOrNullThrowException(nameof(eventID));
            owner.ValidateNotNullThrowException(nameof(owner));
            
            if (_isRegisteringListeners)
            {
                throw new InvalidOperationException("Tried to invoke update during subscription");
            }

            if (!_runners.TryGetValue(eventID, out Runner runner))
            {
                throw new InvalidOperationException($"Tried to invoke invalid update '{eventID}'");
            }
            
            runner.ExecuteBlocking();
        }

        private void _dispose()
        {
            foreach (Runner runner in _runners.Values)
            {
                runner.Dispose();
            }
            
            _runners.Clear();
        }
    }
}