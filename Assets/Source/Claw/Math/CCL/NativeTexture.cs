using System;
using System.Runtime.CompilerServices;
using Beast.Claw.Graphic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Math.CCL
{
    public struct NativeTexture<TValue> : IDisposable
        where TValue : unmanaged
    {
        public NativeArray<TValue> texture;
        public int2 size;

        public NativeTexture(Texture2D texture2D)
        {
            texture = texture2D.GetRawTextureData<TValue>();
            size = texture2D.Size();
        }

        public NativeTexture(NativeArray<TValue> source, int2 textureSize)
        {
            texture = source;
            size = textureSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetPixel(int x, int y)
        {
            return texture[size.x * y + x];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPixel(int x, int y, TValue value)
        {
            texture[size.x * y + x] = value;
        }

        public void Dispose()
        {
            texture.Dispose();
        }
    }
}