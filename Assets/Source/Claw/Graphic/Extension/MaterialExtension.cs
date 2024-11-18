using System.Runtime.CompilerServices;
using Beast.Claw.Validation;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Beast.Claw.Graphic
{
    public static class MaterialExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetFloat2x2(this Material material, string name, float2x2 matrix)
        {
            material.ValidateNotNullThrowException(nameof(material));
            Vector4 vector = new (matrix.c0.x, matrix.c0.y, matrix.c1.x, matrix.c1.y);
            material.SetVector(name, vector);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetFloat2(this Material material, string name, float2 vector)
        {
            material.ValidateNotNullThrowException(nameof(material));
            material.SetVector(name, new Vector4(vector.x, vector.y, 0, 0));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetFloat3(this Material material, string name, float3 vector)
        {
            material.ValidateNotNullThrowException(nameof(material));
            material.SetVector(name, new Vector4(vector.x, vector.y, vector.z, 0));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetFloat4(this Material material, string name, float4 vector)
        {
            material.ValidateNotNullThrowException(nameof(material));
            material.SetVector(name, new Vector4(vector.x, vector.y, vector.z, vector.w));
        }

        public static void SetKeyword(this Material material, string name, bool isEnabled)
        {
            material.ValidateNotNullThrowException(nameof(material));
            LocalKeyword keyword = new (material.shader, name);
            material.SetKeyword(keyword, isEnabled);
        }
    }
}