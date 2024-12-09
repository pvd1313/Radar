using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Claw.Math.CCL
{
    public struct NativeTexture<TValue> : IDisposable
        where TValue : unmanaged
    {
        public NativeArray<TValue> texture;
        public int2 size;

        public NativeTexture(NativeArray<TValue> source, int2 size)
        {
            texture = source;
            this.size = size;
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