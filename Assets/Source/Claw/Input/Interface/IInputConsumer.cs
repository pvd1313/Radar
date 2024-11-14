using Beast.Claw.Common;

namespace Beast.Claw.Input
{
    public interface IInputConsumer : IController
    {
        bool TryCaptureDevice(IInputDevice device, Int32Mask axisMask);

        void UpdateInput(IInputDevice device, Int32Mask axisMask);
    }
}