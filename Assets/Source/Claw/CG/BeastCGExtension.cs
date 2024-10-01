using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.CG
{
    public static class BeastCGExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 WidthHeightFloat(this Texture2D texture2D)
        {
            return new float2(texture2D.width, texture2D.height);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 WidthHeightInt(this Texture2D texture2D)
        {
            return new int2(texture2D.width, texture2D.height);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVector(this Material material, string name, float2 vector)
        {
            material.SetVector(name, new Vector4(vector.x, vector.y, 0, 0));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVector(this Material material, string name, float3 vector)
        {
            material.SetVector(name, new Vector4(vector.x, vector.y, vector.z, 0));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVector(this Material material, string name, float4 vector)
        {
            material.SetVector(name, new Vector4(vector.x, vector.y, vector.z, vector.w));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVector(this Material material, string name, int2 vector)
        {
            material.SetVector(name, new Vector4(vector.x, vector.y, 0, 0));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVector(this Material material, string name, int3 vector)
        {
            material.SetVector(name, new Vector4(vector.x, vector.y, vector.z, 0));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVector(this Material material, string name, int4 vector)
        {
            material.SetVector(name, new Vector4(vector.x, vector.y, vector.z, vector.w));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVector(this MaterialPropertyBlock propertyBlock, string name, float2 vector)
        {
            propertyBlock.SetVector(name, new Vector4(vector.x, vector.y, 0, 0));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVector(this MaterialPropertyBlock propertyBlock, string name, float3 vector)
        {
            propertyBlock.SetVector(name, new Vector4(vector.x, vector.y, vector.z, 0));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVector(this MaterialPropertyBlock propertyBlock, string name, float4 vector)
        {
            propertyBlock.SetVector(name, new Vector4(vector.x, vector.y, vector.z, vector.w));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVector(this MaterialPropertyBlock propertyBlock, string name, int2 vector)
        {
            propertyBlock.SetVector(name, new Vector4(vector.x, vector.y, 0, 0));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVector(this MaterialPropertyBlock propertyBlock, string name, int3 vector)
        {
            propertyBlock.SetVector(name, new Vector4(vector.x, vector.y, vector.z, 0));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVector(this MaterialPropertyBlock propertyBlock, string name, int4 vector)
        {
            propertyBlock.SetVector(name, new Vector4(vector.x, vector.y, vector.z, vector.w));
        }
    }
}