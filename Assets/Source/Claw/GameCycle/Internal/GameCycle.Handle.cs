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
        
            public void ListenOperation(string eventID, Func<OperationContext, UniTask<OperationResult>> operation)
            {
                _gameCycle._listenOperation(currentListener, eventID, operation);
            }

            public void ListenUpdate(string eventID, Action<OperationContext> update)
            {
                _gameCycle._listenUpdate(currentListener, eventID, update);
            }
        }
    }
}