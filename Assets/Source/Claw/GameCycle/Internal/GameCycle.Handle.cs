using System;
using Beast.Claw.Common;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    partial class GameCycle
    {
        private class Handle : IGameCycleHandle
        {
            public IController currentListener;
        
            private readonly GameCycle _gameCycle;

            public Handle(GameCycle gameCycle)
            {
                _gameCycle = gameCycle;
            }
        
            public void ListenConcurrent(string eventID, Func<ConcurrentContext, UniTask> action)
            {
                eventID.ValidateNotEmptyOrNullThrowException(nameof(eventID));
                action.ValidateNotNullThrowException(nameof(action));
                
                if (Build.IS_DEVELOPMENT)
                {
                    if (currentListener != action.Target)
                    {
                        string message = $"'{currentListener.GetType().FullName}' ({eventID}) tried to subscribe a method " +
                                         $"that is implemented not inside '{currentListener.GetType().FullName}'";
                        throw new InvalidOperationException(message);
                    }
                }

                if (!_gameCycle._runners.TryGetValue(eventID, out Runner runner))
                {
                    throw new InvalidOperationException($"Unknown event '{eventID}'");
                }

                if (Build.IS_DEVELOPMENT)
                {
                    if (runner.Operation.flow != EActionFlowType.Concurrent)
                    {
                        throw new InvalidOperationException("Wrong event type for subscription");
                    }
                }
                
                runner.AddListener(new Runner.Listener
                {
                    target = currentListener,
                    concurrent = action
                });
            }

            public void ListenBlocking(string eventID, Action<BlockingContext> action)
            {
                eventID.ValidateNotEmptyOrNullThrowException(nameof(eventID));
                action.ValidateNotNullThrowException(nameof(action));
                
                if (Build.IS_DEVELOPMENT)
                {
                    if (currentListener != action.Target)
                    {
                        string message = $"'{currentListener.GetType().FullName}' ({eventID}) tried to subscribe a method " +
                                         $"that is implemented not inside '{currentListener.GetType().FullName}'";
                        throw new InvalidOperationException(message);
                    }
                }
                
                if (!_gameCycle._runners.TryGetValue(eventID, out Runner runner))
                {
                    throw new InvalidOperationException($"Unknown event '{eventID}'");
                }
                
                if (Build.IS_DEVELOPMENT)
                {
                    if (runner.Operation.flow != EActionFlowType.Blocking)
                    {
                        throw new InvalidOperationException("Wrong event type for subscription");
                    }
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