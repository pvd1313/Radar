using Beast.Claw.Common;

namespace Beast.Claw.Input
{
    public interface IInputManager : IController
    {
        void RegisterConsumer(IInputConsumer consumer, EInputAxis modes, int priority);
        
        bool TryFindConsumer(IInputDevice device, EInputAxis axis, out ConsumerInfo info);
    }
}