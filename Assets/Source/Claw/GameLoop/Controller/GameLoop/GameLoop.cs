using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.Validation;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Beast.Claw.GameLoop
{
    public partial class GameLoop
    {
        private readonly GameLoopConfig _config;
        private readonly DiContainer _container;

        private Dictionary<string, Runner> _runners;
        private List<string> _operationIds;

        private bool _initialized;
        private bool _isRegisteringListeners;
        private int _listenerCount;

        public GameLoop(Proxy proxy, GameLoopConfig config, DiContainer container)
        {
            _config = config;
            _container = container;

            proxy.Inject(this);
        }
        
        private void _initialize(GameLoopSetup input)
        {
            if (_initialized)
            {
                throw new InvalidOperationException("Already initialized");
            }
            
            input.ValidateThrowException(nameof(input));
            
            _initialized = true;

            _runners = new Dictionary<string, Runner>();
            _operationIds = new List<string>(input.actions.Count);

            foreach (ActionProperties operation in input.actions)
            {
                _runners[operation.id] = new Runner(_config, operation);
                _operationIds.Add(operation.id);
            }
        }

        private void _registerListeners()
        {
            _isRegisteringListeners = true;
            
            Handle handle = new (this);
            
            List<IGameLoopActor> listeners = _container.TryResolve<List<IGameLoopActor>>();
            
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
                IGameLoopActor subscriber = listeners[i];
                
                try
                {
                    handle.currentListener = subscriber;
                    subscriber.Register(handle);
                }
                catch (Exception e)
                {
                    BDebug.LogException(e);

                    throw;
                }
            }

            _listenerCount = listeners.Count;

            _isRegisteringListeners = false;
        }

        private async UniTask<EActionStatus> _executeConcurrent(IController owner, string eventID) 
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

        private void _executeBlocking(IController owner, string eventID)
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
            
            runner.ExecuteBlocking(owner);
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