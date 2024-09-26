using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Common
{
    public static class BeastUtil
    {
        public static Vector3 AsVector3(this float2 value)
        {
            return new Vector3(value.x, value.y, 0);
        }
        
        public static float2 AsFloat2(this Vector3 value)
        {
            return new float2(value.x, value.y);
        }
        
        public static float3 AsFloat3(this Vector3 value)
        {
            return new float3(value.x, value.y, value.z);
        }
        public static float3 AsFloat3(this Vector2 value)
        {
            return new float3(value.x, value.y,0);
        }
        
        public static float2 AsFloat2(this float3 value)
        {
            return new float2(value.x, value.y);
        }
    }
}