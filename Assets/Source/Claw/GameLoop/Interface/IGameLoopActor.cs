using Beast.Claw.Common;

namespace Beast.Claw.GameLoop
{
    public interface IGameLoopActor : IController
    {
        void Register(IGameLoopHandle handle);
    }
}