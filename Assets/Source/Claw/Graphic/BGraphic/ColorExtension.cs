using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Graphic
{
    public static class ColorExtension
    {
        public static half4 AsHalf4(this Color color)
        {
            return new half4((half)color.r, (half)color.g, (half)color.b, (half)color.a);
        }
    }
}