using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Beast.Claw.Mathematics
{
    partial class BMath
    {
        public static class Quaternion
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static quaternion EulerDeg(float x, float y, float z, math.RotationOrder order = math.RotationOrder.ZXY)
            {
                return quaternion.Euler(x * Deg2Rad, y * Deg2Rad, z * Deg2Rad, order);
            }
        }
    }
}