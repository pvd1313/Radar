using System;
using Cysharp.Threading.Tasks;
using UnityEngine.Rendering;

namespace Beast.Claw.GameCycle
{
    partial class GameCycle
    {
        private class Handle : IGameCycleHandle
        {
            public object currentListener;
        
            private readonly GameCycle _gameCycle;
            private readonly bool _preventExternMethod;

            public Handle(GameCycle gameCycle)
            {
                _gameCycle = gameCycle;
                _preventExternMethod = gameCycle._config.preventExternSubscription;
            }
        
            public void ListenOperation(string eventID, Func<ActionContext, UniTask<ActionResult>> action)
            {
                eventID.ValidateNotEmptyOrNullThrowException(nameof(eventID));
                action.ValidateNotNullThrowException(nameof(action));
                
                if (_preventExternMethod && currentListener != action.Target)
                {
                    string message = $"'{currentListener.GetType().FullName}' ({eventID}) tried to subscribe a method " +
                                     $"that is implemented not inside '{currentListener.GetType().FullName}'";
                    throw new InvalidOperationException(message);
                }

                if (!_gameCycle._runners.TryGetValue(eventID, out Runner runner))
                {
                    throw new InvalidOperationException($"Unknown event '{eventID}'");
                }

                if (runner.Operation.flow != EActionFlowType.Concurrent)
                {
                    throw new InvalidOperationException("Wrong event type for subscription");
                }
                
                runner.AddListener(new Runner.Listener
                {
                    target = currentListener,
                    concurrent = action
                });
            }

            public void ListenUpdate(string eventID, Action<ActionContext> action)
            {
                eventID.ValidateNotEmptyOrNullThrowException(nameof(eventID));
                action.ValidateNotNullThrowException(nameof(action));
                
                if (_preventExternMethod && currentListener != action.Target)
                {
                    string message = $"'{currentListener.GetType().FullName}' ({eventID}) tried to subscribe a method " +
                                     $"that is implemented not inside '{currentListener.GetType().FullName}'";
                    throw new InvalidOperationException(message);
                }
                
                if (!_gameCycle._runners.TryGetValue(eventID, out Runner runner))
                {
                    throw new InvalidOperationException($"Unknown event '{eventID}'");
                }
                
                if (runner.Operation.flow != EActionFlowType.Blocking)
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