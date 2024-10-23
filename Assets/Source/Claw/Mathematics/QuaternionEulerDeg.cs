using Unity.Mathematics;

namespace Beast.Claw.Mathematics
{
    public struct QuaternionEulerDeg
    {
        public float x;
        public float y;
        public float z;
        public math.RotationOrder order;
        
        public QuaternionEulerDeg(float x, float y, float z, math.RotationOrder order = math.RotationOrder.ZXY)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.order = order;
        }

        public static implicit operator quaternion(QuaternionEulerDeg euler)
        {
            float x = euler.x * BMath.DEG2_RAD;
            float y = euler.y * BMath.DEG2_RAD;
            float z = euler.z * BMath.DEG2_RAD;

            return quaternion.Euler(x, y, z, euler.order);
        }
    }
}