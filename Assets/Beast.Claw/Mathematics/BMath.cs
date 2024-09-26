using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Beast.Claw.Mathematics
{
    public static partial class BMath
    {
        public const float Deg2Rad = 0.017453292f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Max(float x, float y)
        {
            return math.max(x, y);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Normalize(float2 x)
        {
            return math.normalize(x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 NormalizeSafe(float2 x)
        {
            return math.normalizesafe(x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(float2 a,float2 b)
        {
            return math.distance(a,b);
        }
            
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Magnitude(this float2 a)
        {
            return math.length(a);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Magnitude(this float3 a)
        {
            return math.length(a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Lerp(Float2MinMax range, float t)
        {
            return math.lerp(range.min, range.max, t);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Lerp(float min, float max, float t)
        {
            return math.lerp(min, max, t);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp01(float x)
        {
            return math.clamp(x, 0, 1);
        }
    }
}