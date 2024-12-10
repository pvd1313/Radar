using System.Runtime.CompilerServices;
using Noesis;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.UI
{
    public static  class NoesisElementExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetMaterial(this Rectangle rectangle, Material material)
        {
            TextureSource textureSource = new(material.mainTexture);
            ImageBrush brush = new(textureSource);
            rectangle.Fill = brush;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 GetActualSize(this FrameworkElement element)
        {
            return new float2(element.ActualWidth, element.ActualHeight);
        }
    }
}