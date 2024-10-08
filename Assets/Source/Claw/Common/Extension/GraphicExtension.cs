using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Common
{
    public static class GraphicExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 WidthHeightFloat(this Texture texture)
        {
            return new float2(texture.width, texture.height);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 WidthHeightInt(this Texture texture)
        {
            return new int2(texture.width, texture.height);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetMatrix(this Material material, string name, float2x2 matrix)
        {
            Vector4 vector = new (matrix.c0.x, matrix.c0.y, matrix.c1.x, matrix.c1.y);
            material.SetVector(name, vector);
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