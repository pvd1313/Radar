using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Beast.Claw.Math
{
    public static class QuaternionExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Mul(this quaternion quaternion, float3 vector)
        {
            return math.mul(quaternion, vector);
        }
    }
}