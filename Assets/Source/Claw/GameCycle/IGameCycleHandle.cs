using System;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public interface IGameCycleHandle
    {
        void ListenOperation<TOwner>(
            TOwner owner, 
            string eventID, 
            Func<UniTask<OperationResult>> operation) where TOwner : class, IGameCycleListener;

        void ListenUpdate<TOwner>(
            TOwner owner, 
            string eventID, 
            Action update) where TOwner : class, IGameCycleListener;
    }
}