using System;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public interface IGameCycleHandle
    {
        void ListenOperation(string eventID, Func<ActionContext, UniTask<ActionResult>> action);

        void ListenUpdate(string eventID, Action<ActionContext> action);
    }
}