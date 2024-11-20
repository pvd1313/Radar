using Unity.Mathematics;

namespace Beast.Claw.Input
{
    public struct ZoomAxis
    {
        public static ZoomAxis Inactive { get; } = default;
        
        public float delta;
        public float2 position;
    }
}