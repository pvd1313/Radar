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

        public float2 ComputePoint(float t)
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
        
        public float2 ComputeDerivative(float t)
        {
            float u = 1 - t;
            return (3 * u * u) * (handle0 - point0) + 
                   (6 * u * t) * (handle1 - handle0) + 
                   (3 * t * t) * (point1 - handle1);
        }
        
        public float2 ComputeLength()
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
            
            float2 velocity = ComputeDerivative(t);

            float speed = BMath.Length(velocity);

            length += weight * speed;
        }
    }
}