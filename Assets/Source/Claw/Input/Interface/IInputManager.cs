using Beast.Claw.Common;

namespace Beast.Claw.Input
{
    public interface IInputManager : IController
    {
        void RegisterConsumer(IInputConsumer consumer, int priority);
        
        bool TryFindConsumer(PackedAxis axis, out ConsumerInfo info);
    }
}