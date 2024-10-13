using Unity.Mathematics;

namespace Beast.Claw.Math
{
    public struct Float4x4TRS
    {
        public float3 translation;
        public quaternion rotation;
        public float3 scale;
        
        public static implicit operator float4x4(Float4x4TRS trs)
        {
            return float4x4.TRS(trs.translation, trs.rotation, trs.scale);
        }
    }
}