using Unity.Mathematics;

namespace Beast.Claw.Mathematics
{
    public struct QuaternionEulerRad
    {
        public float x;
        public float y;
        public float z;
        public math.RotationOrder order;
        
        public QuaternionEulerRad(float x, float y, float z, math.RotationOrder order = math.RotationOrder.ZXY)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.order = order;
        }

        public static implicit operator quaternion(QuaternionEulerRad euler)
        {
            float x = euler.x;
            float y = euler.y;
            float z = euler.z;

            return quaternion.Euler(x, y, z, euler.order);
        }
    }
}