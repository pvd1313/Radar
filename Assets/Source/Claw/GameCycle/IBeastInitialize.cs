using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public interface IBeastInitialize
    {
        public UniTask<BeastInitializationResult> Initialize();
    }
}