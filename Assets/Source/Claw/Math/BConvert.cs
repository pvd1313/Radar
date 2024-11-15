using System.Runtime.CompilerServices;
using UnityEngine;

namespace Beast.Claw.Math
{
    public static class BConvert
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ColorToHalf2(Color color, out ushort half0, out ushort half1)
        {
            byte r = (byte)(color.r * 127.0f);
            byte g = (byte)(color.g * 127.0f);
            byte b = (byte)(color.b * 127.0f);
            byte a = (byte)(color.a * 127.0f);
            
            half0 = (ushort)((r << 7) | g);
            half1 = (ushort)((b << 7) | a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FloatToHalf2(float value, out ushort half0, out ushort half1)
        {
            uint sign = value < 0 ? 32767u : 0;
            
            half0 = (ushort)(BMath.FloorToInt(BMath.Abs(value)) + sign);
            half1 = FloatToHalf(value % 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort FloatToHalf(float value)
        {
            return Mathf.FloatToHalf(value);
        }
    }
}