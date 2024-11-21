using System.Runtime.CompilerServices;
using Beast.Claw.Attributes.Annotation;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Graphic
{
    [ExtensionClass]
    public static class TextureExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 Size(this Texture texture)
        {
            return new int2(texture.width, texture.height);
        }
    }
}