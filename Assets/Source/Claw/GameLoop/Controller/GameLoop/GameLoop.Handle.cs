using System;
using Beast.Claw.Common;
using Beast.Claw.Validation;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameLoop
{
    partial class GameLoop
    {
        private class Handle : IGameLoopHandle
        {
            public IController currentListener;
        
            private readonly GameLoop _gameLoop;

            public Handle(GameLoop gameLoop)
            {
                _gameLoop = gameLoop;
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

                if (!_gameLoop._runners.TryGetValue(eventID, out Runner runner))
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
                
                runner.AddActor(new Runner.Actor
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
                
                if (!_gameLoop._runners.TryGetValue(eventID, out Runner runner))
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
                
                runner.AddActor(new Runner.Actor
                {
                    target = currentListener,
                    blocking = action
                });
            }
        }
    }
}