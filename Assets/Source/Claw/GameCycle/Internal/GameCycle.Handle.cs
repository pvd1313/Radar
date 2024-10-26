using System;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    partial class GameCycle
    {
        private class Handle : IGameCycleHandle
        {
            public object currentListener;
        
            private readonly GameCycle _gameCycle;

            public Handle(GameCycle gameCycle)
            {
                _gameCycle = gameCycle;
            }
        
            public void ListenOperation(string eventID, Func<OperationContext, UniTask<OperationResult>> action)
            {
                eventID.ValidateNotEmptyOrNullThrowException(nameof(eventID));
                action.ValidateNotNullThrowException(nameof(action));
                
                if (currentListener != action.Target)
                {
                    string message = $"'{currentListener.GetType().FullName}' ({eventID}) tried to subscribe a method " +
                                     $"that is implemented not inside '{currentListener.GetType().FullName}'";
                    throw new InvalidOperationException(message);
                }

                if (!_gameCycle._runners.TryGetValue(eventID, out Runner runner))
                {
                    throw new InvalidOperationException($"Unknown event '{eventID}'");
                }

                if (runner.Operation.flow != EOperationFlowType.Concurrent)
                {
                    throw new InvalidOperationException("Wrong event type for subscription");
                }
                
                runner.AddListener(new Runner.Listener
                {
                    target = currentListener,
                    concurrent = action
                });
            }

            public void ListenUpdate(string eventID, Action<OperationContext> action)
            {
                eventID.ValidateNotEmptyOrNullThrowException(nameof(eventID));
                action.ValidateNotNullThrowException(nameof(action));
                
                if (currentListener != action.Target)
                {
                    string message = $"'{currentListener.GetType().FullName}' ({eventID}) tried to subscribe a method " +
                                     $"that is implemented not inside '{currentListener.GetType().FullName}'";
                    throw new InvalidOperationException(message);
                }
                
                if (!_gameCycle._runners.TryGetValue(eventID, out Runner runner))
                {
                    throw new InvalidOperationException($"Unknown event '{eventID}'");
                }
                
                if (runner.Operation.flow != EOperationFlowType.Blocking)
                {
                    throw new InvalidOperationException("Wrong event type for subscription");
                }
                
                runner.AddListener(new Runner.Listener
                {
                    target = currentListener,
                    blocking = action
                });
            }
        }
    }
}