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
        public static void SetVector(this Material material, string name, float2 vector)
        {
            material.SetVector(name, (Vector2)vector);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVector(this Material material, string name, float3 vector)
        {
            material.SetVector(name, (Vector3)vector);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVector(this Material material, string name, float4 vector)
        {
            material.SetVector(name, vector);
        }
    }
}