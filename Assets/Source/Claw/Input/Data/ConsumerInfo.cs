using Beast.Claw.Common;

namespace Beast.Claw.Input
{
    public struct ConsumerInfo
    {
        public Int32Mask axisMask;
        public IInputConsumer consumer;
        public int priority;
    }
}