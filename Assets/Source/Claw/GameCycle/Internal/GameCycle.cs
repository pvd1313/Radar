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
        private HashSet<object> _registeredListeners;
        private List<string> _allOperations;

        private bool _initialized;
        private bool _isRegisteringListeners;

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

            _registeredListeners = new HashSet<object>();
            _runners = new Dictionary<string, Runner>();
            _allOperations = new List<string>(input.operations.Count);

            foreach (OperationInfo operation in input.operations)
            {
                _runners[operation.id] = new Runner(_config, operation);
                _allOperations.Add(operation.id);
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
                    BDebug.LogException(this, e, $"Exception during registration ({subscriber.GetType()})");

                    throw;
                }
                finally
                {
                    _registeredListeners.Add(subscriber);
                }
            }
            
            _isRegisteringListeners = false;
        }

        private async UniTask<OperationResult> _execute<TOwner>(TOwner owner, string eventID) 
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

            return await runner.ExecuteConcurrent();
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

        public void Dispose()
        {
            foreach (Runner runner in _runners.Values)
            {
                runner.Dispose();
            }
            
            _runners.Clear();
        }
    }
}