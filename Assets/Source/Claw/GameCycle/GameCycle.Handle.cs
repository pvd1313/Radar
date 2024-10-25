using System;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    partial class GameCycle
    {
        private class GameCycleHandle : IGameCycleHandle
        {
            public object currentListener;
        
            private readonly GameCycle _gameCycle;

            public GameCycleHandle(GameCycle gameCycle)
            {
                _gameCycle = gameCycle;
            }
        
            public void ListenOperation(string eventID, Func<OperationContext, UniTask<OperationResult>> operation)
            {
                _gameCycle._listenOperation(currentListener, eventID, operation);
            }

            public void ListenUpdate(string eventID, Action<UpdateContext> update)
            {
                _gameCycle._listenUpdate(currentListener, eventID, update);
            }
        }
    }
}