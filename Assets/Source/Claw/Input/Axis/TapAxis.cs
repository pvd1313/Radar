using Unity.Mathematics;

namespace Beast.Claw.Input
{
    public struct TapAxis
    {
        public static TapAxis Inactive { get; } = default;
        
        public int tapCount;
        public float2 position;

        public TapAxis(float2 position, int tapCount)
        {
            this.position = position;
            this.tapCount = tapCount;
        }
    }
}