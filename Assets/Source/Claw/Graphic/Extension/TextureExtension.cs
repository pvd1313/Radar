using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Graphic
{
    public static class TextureExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 Size(this Texture texture)
        {
            return new int2(texture.width, texture.height);
        }
    }
}