using Beast.Claw.Common;

namespace Beast.Claw.Input
{
    public interface IInputConsumer : IController
    {
        public EInputAxis SupportedAxis { get; }
        
        bool TryCaptureAxis(PackedAxis axis);

        void UpdateAxis(PackedAxis axis);
    }
}