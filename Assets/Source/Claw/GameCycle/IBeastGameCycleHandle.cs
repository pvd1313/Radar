using System;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public interface IBeastGameCycleHandle
    {
        void ListenOperation<TOwner>(
            TOwner owner, 
            string eventID, 
            Func<UniTask<BeastInitializationResult>> operation) where TOwner : class, IBeastGameCycleListener;

        void ListenUpdate<TOwner>(
            TOwner owner, 
            string eventID, 
            Action update) where TOwner : class, IBeastGameCycleListener;
    }
}