using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Math
{
    public static class VectorExtensions
    {
        public static float3 XY0(this float2 vector)
        {
            return new float3(vector.x, vector.y, 0);
        }
        
        public static float3 X0Y(this float2 vector)
        {
            return new float3(vector.x, 0, vector.y);
        }

        public static float2 XY(this Vector3 vector)
        {
            return new float2(vector.x, vector.y);
        }
    }
}