using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Mathematics
{
    public static class VectorExtensions
    {
        public static float3 XY0(this float2 vector)
        {
            return new Vector3(vector.x, vector.y, 0);
        }
        
        public static float3 X0Y(this float2 vector)
        {
            return new Vector3(vector.x, 0, vector.y);
        }
    }
}