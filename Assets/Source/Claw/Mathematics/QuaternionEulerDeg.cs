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
            float x = euler.x * BMath.Deg2Rad;
            float y = euler.y * BMath.Deg2Rad;
            float z = euler.z * BMath.Deg2Rad;

            return quaternion.Euler(x, y, z, euler.order);
        }
    }
}