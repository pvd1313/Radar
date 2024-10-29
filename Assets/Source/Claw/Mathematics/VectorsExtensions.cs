using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Mathematics
{
    public static class VectorsExtensions
    {
        public static Vector3 XY0(this float2 value)
        {
            return new Vector3(value.x, value.y, 0);
        }
        
        public static float2 XY(this Vector3 value)
        {
            return new float2(value.x, value.y);
        }
    }
}