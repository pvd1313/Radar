using Beast.Claw.Common;
using Beast.Claw.Math;

namespace Beast.Claw.Input
{
    public interface IInputConsumer : IController
    {
        public Int32Mask SupportedAxis { get; }
        
        bool TryCaptureAxis(PackedAxis axis);

        void UpdateAxis(PackedAxis axis);
    }
}