using Beast.Claw.Math;

namespace Beast.Claw.Input
{
    public struct ConsumerInfo
    {
        public Int32Mask axisMask;
        public IInputConsumer consumer;
        public int priority;

        public void UpdateAxis(PackedAxis axis)
        {
            consumer.UpdateAxis(axis);
        }

        public bool TryCaptureAxis(PackedAxis axis)
        {
            return consumer.TryCaptureAxis(axis);
        }
    }
}