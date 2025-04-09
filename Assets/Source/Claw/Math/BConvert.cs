using System.Runtime.CompilerServices;
using Beast.Claw.Attributes.Annotation;
using UnityEngine;

namespace Beast.Claw.Math
{
    [StaticUtility]
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
            value = BMath.Clamp(value, -32767, 32767);
            
            uint sign = value < 0 ? 32767u : 0;
            
            half0 = (ushort)(BMath.FloorToInt(BMath.Abs(value)) + sign);
            half1 = FloatToHalf(value % 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort FloatToHalf(float value)
        {
            return Mathf.FloatToHalf(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort SByte2ToHalf(int x, int y)
        {
            x = BMath.Clamp(x, -63, +63);
            y = BMath.Clamp(y, -63, +63);
            
            if (x < 0)
            {
                x = x * -1 + 63;
            }

            if (y < 0)
            {
                y = y * -1 + 63;
            }
            
            return Mathf.FloatToHalf(x + y / 128.0f);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort UByte2ToHalf(int x, int y)
        {
            x = BMath.Clamp(x, 0, 127);
            y = BMath.Clamp(y, 0, 127);
            
            return Mathf.FloatToHalf(x + y / 128.0f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int UShort2ToInt(ushort a, ushort b)
        {
            return (a << 16) | b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IntToUShort2(int value, out ushort a, out ushort b)
        {
            a = (ushort)(value >> 16);
            b = (ushort)(value & 0xFFFF);
        }
    }
}