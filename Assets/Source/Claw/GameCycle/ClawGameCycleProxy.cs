using System;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public class ClawGameCycleProxy : IBeastGameCycle
    {
        private ClawGameCycle _gameCycle;

        public void Inject(ClawGameCycle gameCycle)
        {
            _gameCycle = gameCycle;
        }
        
        public bool TryInitializeSelf(BeastGameCycleParameters input)
        {
            return _gameCycle.TryInitializeSelf(input);
        }

        public void ListenOperation<TOwner>(TOwner owner, string eventID, Func<UniTask<BeastInitializationResult>> operation) 
            where TOwner : class, IBeastGameCycleListener
        {
            _gameCycle.ListenOperation(owner, eventID, operation);
        }

        public void ListenUpdate<TOwner>(TOwner owner, string eventID, Action update) 
            where TOwner : class, IBeastGameCycleListener
        {
            _gameCycle.ListenUpdate(owner, eventID, update);
        }

        public void Update<TOwner>(TOwner owner, string eventID)
        {
            _gameCycle.Update(owner, eventID);
        }

        public async UniTask<BeastInitializationResult> Execute<TOwner>(TOwner owner, string eventID)
        {
            return await _gameCycle.Execute(owner, eventID);
        }
    }
}