using Beast.Claw.Common;

namespace Beast.Claw.GameCycle
{
    public interface IGameCycleListener : IController
    {
        void Register(IGameCycleHandle handle);
    }
}