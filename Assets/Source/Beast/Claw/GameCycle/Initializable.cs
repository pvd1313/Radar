using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public interface Initializable
    {
        public UniTask<InitializationResult> Initialize();
    }
}