using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public interface IGameCycle : IGameCycleHandle
    {
        public bool TryInitializeSelf(GameCycleParameters input);
        
        UniTask<OperationResult> Execute<TOwner>(TOwner owner, string eventID);

        public void Update<TOwner>(TOwner owner, string eventID);
    }
}