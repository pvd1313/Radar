using System.Runtime.CompilerServices;
using Beast.Claw.Attributes.Annotation;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Math
{
    [StaticUtility]
    public static class BMath
    {
        public const float DEG2_RAD = 0.017453292f;
        public const float RAD_2DEG = 57.29578f;
        public const float TWO_PI = 2 * math.PI;
        public const float PI = math.PI;
        public const float EPSILON = math.EPSILON;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sin(float angleRag)
        {
            return math.sin(angleRag);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cos(float angleRad)
        {
            return math.cos(angleRad);
        }

        /// <summary>
        /// OppositeEdge / Diagonal
        /// </summary>
        /// <returns>radians</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ASin(float x)
        {
            return math.asin(x);
        }

        /// <summary>
        /// NearEdge / Diagonal
        /// </summary>
        /// <returns>radians</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ACos(float x)
        {
            return math.acos(x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Abs(int a)
        {
            return math.abs(a);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Abs(float a)
        {
            return math.abs(a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(float a, float b)
        {
            return Mathf.Approximately(a, b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOf2(int x)
        {
            return math.ispow2(x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(float4 a, float4 b)
        {
            return math.dot(a, b);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(float2 a, float2 b)
        {
            return Mathf.Approximately(a.x, b.x) &&
                   Mathf.Approximately(a.y, b.y);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(float3 a, float3 b)
        {
            return Mathf.Approximately(a.x, b.x) &&
                   Mathf.Approximately(a.y, b.y) &&
                   Mathf.Approximately(a.z, b.z);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(float4 a, float4 b)
        {
            return Mathf.Approximately(a.x, b.x) &&
                   Mathf.Approximately(a.y, b.y) &&
                   Mathf.Approximately(a.z, b.z) &&
                   Mathf.Approximately(a.w, b.w);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Max(float x, float y)
        {
            return math.max(x, y);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Max(uint x, uint y)
        {
            return math.max(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Max(int x, int y)
        {
            return math.max(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Min(float x, float y)
        {
            return math.min(x, y);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Min(int x, int y)
        {
            return math.min(x, y);
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
        public static float DistanceSq(float2 a,float2 b)
        {
            return math.distancesq(a,b);
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
        public static float Lerp(FloatMinMax range, float t)
        {
            return math.lerp(range.min, range.max, t);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Lerp(float min, float max, float t)
        {
            return math.lerp(min, max, t);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Lerp(float2 min, float2 max, float t)
        {
            return math.lerp(min, max, t);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 MoveTowards(float2 current, float2 target, float t)
        {
            float2 delta = target - current;
            float distance = math.length(delta);

            if (distance <= t || distance == 0f)
            {
                return target;
            }

            float2 direction = math.normalize(delta);
            float2 step = direction * t;
            return current + step;
        }

        /// <summary>
        /// Inverse lerp / unlerp.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InverseLerp(FloatMinMax range, float value)
        {
            return math.unlerp(range.min, range.max, value);
        }
        
        /// <summary>
        /// Inverse lerp / unlerp.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InverseLerp(float min, float max, float value)
        {
            return math.unlerp(min, max, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Floor(float a)
        {
            return math.floor(a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Floor(float2 vector)
        {
            return math.floor(vector);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Floor(float3 vector)
        {
            return math.floor(vector);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 Floor(float4 vector)
        {
            return math.floor(vector);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Log2(float a)
        {
            return math.log2(a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IntFloorLog2(int a)
        {
            return math.floorlog2(a);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double2 Floor(double2 vector)
        {
            return math.floor(vector);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Floor(double a)
        {
            return math.floor(a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Round(float a)
        {
            return math.round(a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FloorToInt(float a)
        {
            return (int)math.floor(a);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 FloorToInt(float2 a)
        {
            return (int2)math.floor(a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Ceil(float x)
        {
            return math.ceil(x);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Ceil(float2 x)
        {
            return new float2(math.ceil(x.x), math.ceil(x.y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Ceil(double x)
        {
            return math.ceil(x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CeilToInt(float x)
        {
            return (int)math.ceil(x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp01(float x)
        {
            return math.clamp(x, 0, 1);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Clamp(float2 value, float min, float max)
        {
            return math.clamp(value, min, max);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 Clamp(int2 value, int min, int max)
        {
            return math.clamp(value, min, max);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(float value, float min, float max)
        {
            return math.clamp(value, min, max);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(int value, int min, int max)
        {
            return math.clamp(value, min, max);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sqrt(float x)
        {
            return math.sqrt(x);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Rotate(float x, float y, float sin, float cos)
        {
            return new float2
            {
                x = x * cos - y * sin,
                y = x * sin + y * cos
            };
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Rotate(float2 vector, float sin, float cos)
        {
            return Rotate(vector.x, vector.y, sin, cos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SinCos(float angleRad, out float sin, out float cos)
        {
            sin = math.sin(angleRad);
            cos = math.cos(angleRad);
        }
    }
}