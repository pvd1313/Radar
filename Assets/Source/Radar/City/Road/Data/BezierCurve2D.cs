using Beast.Claw.Math;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    public struct BezierCurve2D
    {
        public float2 point0;
        public float2 point1;
        public float2 handle0;
        public float2 handle1;

        public double2 ComputePointDouble(double t)
        {
            double u = 1 - t;
            double uu = u * u;
            double uuu = uu * u;
            double tt = t * t;
            double ttt = tt * t;

            return (uuu * (double2)point0) + 
                   (3 * uu * t * (double2)handle0) + 
                   (3 * u * tt * (double2)handle1) + 
                   (ttt * (double2)point1);
        }
        
        public float2 ComputePointFloat(float t)
        {
            float u = 1 - t;
            float uu = u * u;
            float uuu = uu * u;
            float tt = t * t;
            float ttt = tt * t;

            return (uuu * point0) + 
                   (3 * uu * t * handle0) + 
                   (3 * u * tt * handle1) + 
                   (ttt * point1);
        }
        
        public float ComputeLength()
        {
            float length = 0;

            _gaussianQuadrature(-0.90618f, 0.23693f, ref length);
            _gaussianQuadrature(-0.53846f, 0.47863f, ref length);
            _gaussianQuadrature(0.0f, 0.56889f, ref length);
            _gaussianQuadrature(0.53846f, 0.47863f, ref length);
            _gaussianQuadrature(0.90618f, 0.23693f, ref length);
            
            return length * 0.5f;
        }

        private void _gaussianQuadrature(float point, float weight, ref float length)
        {
            float t = (point + 1.0f) * 0.5f;
            
            float2 velocity = _computeDerivative(t);

            float speed = BMath.Length(velocity);

            length += weight * speed;
        }
        
        private float2 _computeDerivative(float t)
        {
            float u = 1 - t;
            return (3 * u * u) * (handle0 - point0) + 
                   (6 * u * t) * (handle1 - handle0) + 
                   (3 * t * t) * (point1 - handle1);
        }

        public override string ToString()
        {
            return $"{{P0:{point0}; P1:{point1}; H0:{handle0}; H1:{handle1}}}";
        }
    }
}