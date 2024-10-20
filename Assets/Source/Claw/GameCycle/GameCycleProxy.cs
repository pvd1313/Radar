using System;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public class GameCycleProxy : IGameCycle
    {
        private GameCycle _gameCycle;

        public void Inject(GameCycle gameCycle)
        {
            _gameCycle = gameCycle;
        }
        
        public bool TryInitializeSelf(GameCycleParameters input)
        {
            return _gameCycle.TryInitializeSelf(input);
        }

        public void ListenOperation<TOwner>(TOwner owner, string eventID, Func<UniTask<OperationResult>> operation) 
            where TOwner : class, IGameCycleListener
        {
            _gameCycle.ListenOperation(owner, eventID, operation);
        }

        public void ListenUpdate<TOwner>(TOwner owner, string eventID, Action update) 
            where TOwner : class, IGameCycleListener
        {
            _gameCycle.ListenUpdate(owner, eventID, update);
        }

        public void Update<TOwner>(TOwner owner, string eventID) where TOwner : class
        {
            _gameCycle.Update(owner, eventID);
        }

        public async UniTask<OperationResult> Execute<TOwner>(TOwner owner, string eventID) where TOwner : class
        {
            return await _gameCycle.Execute(owner, eventID);
        }
    }
}