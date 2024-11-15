using System;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameLoop
{
    public interface IGameLoopHandle
    {
        void ListenConcurrent(string eventID, Func<ConcurrentContext, UniTask> action);

        void ListenBlocking(string eventID, Action<BlockingContext> action);
    }
}