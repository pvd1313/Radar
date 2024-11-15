
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameLoop
{
    public struct ConcurrentContext
    {
        public UniTask CancelEmptyMethod()
        {
            throw SubscriptionCancelledException.EmptyMethod;
        }
    }
}