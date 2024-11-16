using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameLoop
{
    public struct ConcurrentContext
    {
        public UniTask Abort(EActionAbortReason reason)
        {
            throw ActionAbortedException.FromReason(reason);
        }
    }
}