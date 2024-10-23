using Unity.Mathematics;
using Matrix4x4 = UnityEngine.Matrix4x4;

namespace Beast.Claw.Mathematics
{
    public struct Float4x4TRS
    {
        public float3 translation;
        public quaternion rotation;
        public float3 scale;
        
        public static implicit operator float4x4(Float4x4TRS matrix)
        {
            return float4x4.TRS(matrix.translation, matrix.rotation, matrix.scale);
        }

        public static implicit operator Matrix4x4(Float4x4TRS matrix)
        {
            return float4x4.TRS(matrix.translation, matrix.rotation, matrix.scale);
        }
    }
}