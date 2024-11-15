using Beast.Claw.Common;

namespace Beast.Claw.GameLoop
{
    public interface IGameLoopListener : IController
    {
        void Register(IGameLoopHandle handle);
    }
}