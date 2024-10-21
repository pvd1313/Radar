using System;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public interface IGameCycleHandle
    {
        void ListenOperation(string eventID, Func<UniTask<OperationResult>> operation);

        void ListenUpdate(string eventID, Action update);
    }
}