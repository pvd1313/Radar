using Unity.Mathematics;

namespace Beast.Claw.Mathematics
{
    public static class MathExtension
    {
        public static float2 Mul(this int2 vector, float scale)
        {
            return new float2(vector.x * scale, vector.y * scale);
        }
        
        public static float2 Div(this int2 vector, float scale)
        {
            return new float2(vector.x / scale, vector.y / scale);
        }
        
        public static float2 Mul(this float2x2 matrix, float2 vector)
        {
            float x = matrix.c0.x * vector.x + matrix.c1.x * vector.y;
            float y = matrix.c0.y * vector.x + matrix.c1.y * vector.y;

            return new float2(x, y);
        }

        public static float2x2 Inverse(this float2x2 matrix)
        {
            float a = matrix.c0.x;
            float b = matrix.c1.x;
            float c = matrix.c0.y;
            float d = matrix.c1.y;
            
            return new float2x2(d, -b, -c, a) * (1 / (a * d - b * c));
        }
        
        public static double2 Multiply(this double2x2 matrix, double2 vector)
        {
            double x = matrix.c0.x * vector.x + matrix.c1.x * vector.y;
            double y = matrix.c0.y * vector.x + matrix.c1.y * vector.y;

            return new double2(x, y);
        }
        
        public static double2x2 Inverse(this double2x2 matrix)
        {
            double a = matrix.c0.x;
            double b = matrix.c1.x;
            double c = matrix.c0.y;
            double d = matrix.c1.y;
            
            return new double2x2(d, -b, -c, a) * (1 / (a * d - b * c));
        }
    }
}