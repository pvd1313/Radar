using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Claw.Input
{
    public struct RawTouchAxis
    {
        public static RawTouchAxis Inactive { get; } = new()
        {
            fingerId = -1,
            position = float2.zero,
            status = EAxisStatus.Inactive
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
        
        public static bool ContainsActiveAxis(NativeSlice<RawTouchAxis> touches)
        {
            for (int i = 0; i < touches.Length; i++)
            {
                if (touches[i].status != EAxisStatus.Inactive)
                {
                    return true;
                }
            }

            return false;
        }
        
        public static bool ContainsAxisWithStatus(NativeSlice<RawTouchAxis> touches, EAxisStatus status)
        {
            for (int i = 0; i < touches.Length; i++)
            {
                if (touches[i].status == status)
                {
                    return true;
                }
            }

            return false;
        }
    }
}