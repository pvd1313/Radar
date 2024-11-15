using Unity.Mathematics;

namespace Beast.Claw.Input
{
    public struct DragAxis
    {
        public static DragAxis Inactive { get; } = new ()
        {
            startPosition = float2.zero,
            position = float2.zero,
            status = EAxisStatus.Disabled
        };
        
        public float2 startPosition;
        public float2 position;
        public EAxisStatus status;

        public static DragAxis StartingAt(float2 position)
        {
            return new DragAxis
            {
                startPosition = position,
                position = position,
                status = EAxisStatus.Starting
            };
        }
    }
}