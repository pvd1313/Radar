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