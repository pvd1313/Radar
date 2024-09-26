using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public interface Disposable
    {
        public UniTask<DispositionResult> Dispose();
    }
}