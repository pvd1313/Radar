using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Mathematics
{
    public static class VectorsExtensions
    {
        public static Vector3 AsVector3(this float2 value)
        {
            return new Vector3(value.x, value.y, 0);
        }
    
    }
}