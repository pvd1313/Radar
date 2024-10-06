using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public interface IBeastGameCycle : IBeastGameCycleHandle
    {
        public bool TryInitializeSelf(BeastGameCycleParameters input);
        
        UniTask<BeastInitializationResult> Execute<TOwner>(TOwner owner, string eventID);

        public void Update<TOwner>(TOwner owner, string eventID);
    }
}