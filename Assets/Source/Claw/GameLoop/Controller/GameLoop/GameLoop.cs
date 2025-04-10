using System;
using System.Collections.Generic;
using Beast.Claw.Attributes;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.Profiling;
using Beast.Claw.ProgressTracker;
using Beast.Claw.Text;
using Beast.Claw.Validation;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameLoop
{
    public partial class GameLoop : Controller, IGameLoop
    {
        public IReadOnlyList<string> AllOperations => _operationIds;
        
        [Required] private readonly GameLoopConfig _config;

        private Dictionary<string, Runner> _runners;
        private HashSet<IGameLoopActor> _registeredActors;
        private List<string> _operationIds;

        private bool _initialized;
        private bool _isRegisteringActors;
        private bool _isExecutingConcurrentAction;

        public void Initialize(GameLoopSetup input)
        {
            if (_initialized)
            {
                throw new InvalidOperationException("Already initialized");
            }
            
            input.ValidateThrowException(nameof(input));
            
            _initialized = true;

            _runners = new Dictionary<string, Runner>();
            _operationIds = new List<string>(input.actions.Count);
            _registeredActors = new HashSet<IGameLoopActor>();

            foreach (ActionProperties operation in input.actions)
            {
                _runners[operation.id] = new Runner(_config, operation);
                _operationIds.Add(operation.id);
            }
        }

        public void RegisterActors(IReadOnlyList<IGameLoopActor> actors)
        {
            actors.ValidateNotNullThrowException(nameof(actors));
            
            _isRegisteringActors = true;
            
            Handle handle = new (this);

            foreach (IGameLoopActor actor in actors)
            {
                if (!_registeredActors.Add(actor))
                {
                    continue;
                }
                
                try
                {
                    handle.currentListener = actor;
                    actor.Register(handle);
                }
                catch (Exception e)
                {
                    BDebug.LogException(e);

                    throw;
                }
            }

            _isRegisteringActors = false;
        }
        
        public IBricksProfiler TryGetProfiler(string operationName)
        {
            if (!_runners.TryGetValue(operationName, out Runner runner))
            {
                return null;
            }

            return runner.Profiler;
        }

        public async UniTask<EActionStatus> ExecuteConcurrent(
            IController owner, 
            string eventID, 
            IProgressTracker progressTracker) 
        {
            eventID.ValidateNotEmptyOrNullThrowException(nameof(eventID));
            owner.ValidateNotNullThrowException(nameof(owner));
            
            if (_isRegisteringActors)
            {
                throw new InvalidOperationException("Tried to invoke operation during registration");
            }

            if (_isExecutingConcurrentAction)
            {
                throw new InvalidOperationException("Tried to invoke concurrent action while another is active");
            }

            if (!_runners.TryGetValue(eventID, out Runner runner))
            {
                throw new InvalidOperationException($"Tried to invoke invalid operation {eventID.Q()}");
            }

            _isExecutingConcurrentAction = true;
            
            EActionStatus result = await runner.ExecuteConcurrent(owner, progressTracker);

            _isExecutingConcurrentAction = false;

            return result;
        }

        public void ExecuteBlocking(IController owner, string eventID)
        {
            eventID.ValidateNotEmptyOrNullThrowException(nameof(eventID));
            owner.ValidateNotNullThrowException(nameof(owner));

            if (_isExecutingConcurrentAction)
            {
                return;
            }
            
            if (_isRegisteringActors)
            {
                throw new InvalidOperationException("Tried to invoke update during subscription");
            }

            if (!_runners.TryGetValue(eventID, out Runner runner))
            {
                throw new InvalidOperationException($"Tried to invoke invalid update {eventID.Q()}");
            }
            
            runner.ExecuteBlocking(owner);
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