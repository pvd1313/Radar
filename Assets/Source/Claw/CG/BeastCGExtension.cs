using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.CG
{
    public static class BeastCGExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 WidthHeight(this Texture2D texture2D)
        {
            return new float2(texture2D.width, texture2D.height);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVector(this Material material, string name, float2 vector2)
        {
            material.SetVector(name, (Vector2)vector2);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVector(this Material material, string name, float3 vector3)
        {
            material.SetVector(name, (Vector3)vector3);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVector(this Material material, string name, float4 vector4)
        {
            material.SetVector(name, vector4);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVector(this MaterialPropertyBlock propertyBlock, string name, float2 vector2)
        {
            propertyBlock.SetVector(name, (Vector2)vector2);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVector(this MaterialPropertyBlock propertyBlock, string name, float3 vector3)
        {
            propertyBlock.SetVector(name, (Vector3)vector3);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVector(this MaterialPropertyBlock propertyBlock, string name, float4 vector4)
        {
            propertyBlock.SetVector(name, vector4);
        }
    }
}