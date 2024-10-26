using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public partial class GameCycle
    {
        private class Runner
        {
            public struct Listener
            {
                public Action<OperationContext> blocking;
                public Func<OperationContext, UniTask<OperationResult>> concurrent;
                public object target;
            }

            public OperationInfo Operation => _operation;
            
            private readonly GameCycleConfig _config;
            private readonly GameCycle _gameCycle;

            private readonly HashSet<object> _singleFireObjects;
            private readonly List<Listener> _listeners;
            private readonly HashSet<object> _listenerHashset;
            private readonly OperationInfo _operation;

            private readonly Dictionary<object, int> _blockingLogicCounters;
            private readonly HashSet<object> _blockingBrokenLogic;
            private bool _blockingLogicAllBroken;

            public Runner(GameCycle gameCycle, GameCycleConfig config, OperationInfo operationInfo)
            {
                config.ValidateNotNullThrowException(nameof(config));
                gameCycle.ValidateNotNullThrowException(nameof(gameCycle));
                operationInfo.ValidateThrowException(nameof(operationInfo));
                
                _config = config;
                _gameCycle = gameCycle;
                _operation = operationInfo;

                if (operationInfo.frequency == EOperationFrequency.OncePerObjectLife)
                {
                    _singleFireObjects = new HashSet<object>();
                }

                if (operationInfo.flow == EOperationFlowType.Blocking)
                {
                    _blockingLogicCounters = new Dictionary<object, int>();
                    _blockingBrokenLogic = new HashSet<object>();
                }
                
                _listeners = new List<Listener>();
                _listenerHashset = new HashSet<object>();
            }

            public void AddListener(Listener listener)
            {
                listener.target.ValidateNotNullThrowException("listener.target");

                if (!_listenerHashset.Add(listener.target))
                {
                    string message =
                        $"'{listener.target.GetType().FullName}' is already listening to '{_operation.id}'";
                    throw new InvalidOperationException(message);
                }

                switch (_operation.flow)
                {
                    case EOperationFlowType.Blocking:
                        listener.blocking.ValidateNotNullThrowException("listener.blocking");
                        break;
                        
                    case EOperationFlowType.Concurrent:
                        listener.concurrent.ValidateNotNullThrowException("listener.concurrent");
                        break;
                }

                if (_operation.order == EOperationOrder.FromFirstToLastInjected)
                {
                    _listeners.Add(listener);
                }
                else
                {
                    _listeners.Insert(0, listener);
                }
            }

            public async UniTask<OperationResult> ExecuteConcurrent()
            {
                if (_operation.flow != EOperationFlowType.Concurrent)
                {
                    throw new InvalidOperationException("Invalid flow type");
                }

                return await new RunnerConcurrent
                {
                    singleFiredObjects = _singleFireObjects,
                    operationID = _operation.id,
                    listeners = _listeners,
                    frequency = _operation.frequency
                }.Run();
            }

            public void ExecuteBlocking()
            {
                if (_operation.flow != EOperationFlowType.Blocking)
                {
                    throw new InvalidOperationException("Invalid flow type");
                }

                if (_blockingLogicAllBroken)
                {
                    return;
                }

                RunnerBlocking runner = new()
                {
                    listeners = _listeners,
                    exceptionStopsExecution = _config.exceptionStopsUpdates,
                    exceptionThreshold = _config.updateExceptionThreshold,
                    operationID = _operation.id,
                    brokenObjects = _blockingBrokenLogic,
                    objectErrorCount = _blockingLogicCounters
                };
                
                runner.Run();

                _blockingLogicAllBroken = runner.allLogicIsBroken;
            }
        }
    }
}