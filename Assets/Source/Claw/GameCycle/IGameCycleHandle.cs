using System;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public interface IGameCycleHandle
    {
        void ListenOperation(string eventID, Func<OperationContext, UniTask<OperationResult>> operation);

        void ListenUpdate(string eventID, Action<UpdateContext> update);
    }
}