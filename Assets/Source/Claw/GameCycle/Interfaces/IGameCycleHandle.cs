using System;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public interface IGameCycleHandle
    {
        void ListenConcurrent(string eventID, Func<ConcurrentContext, UniTask> action);

        void ListenBlocking(string eventID, Action<BlockingContext> action);
    }
}