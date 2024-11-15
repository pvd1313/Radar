using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Claw.Input
{
    public struct RawTouchAxis
    {
        public static RawTouchAxis Disabled { get; } = new()
        {
            fingerId = -1,
            position = float2.zero,
            status = EAxisStatus.Disabled
        };
        
        public float2 position;
        public EAxisStatus status;
        public int fingerId;

        public RawTouchAxis(float2 position, EAxisStatus status, int fingerId)
        {
            this.position = position;
            this.status = status;
            this.fingerId = fingerId;
        }

        public static void DeactivateAll(NativeSlice<RawTouchAxis> touches)
        {
            for (int i = 0; i < touches.Length; i++)
            {
                RawTouchAxis axis = touches[i];
                
                axis.status = EAxisStatus.Disabled;

                touches[i] = axis;
            }
        }
    }
}