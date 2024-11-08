using System.Runtime.CompilerServices;
using UnityEngine;

namespace Beast.Claw.Graphic
{
    public static class ColorExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
    }
}