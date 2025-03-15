using System;
using Unity.Mathematics;

namespace Beast.Claw.Math
{
    public struct ushort2 : IEquatable<ushort2>
    {
        public ushort x;
        public ushort y;

        public ushort2(int x, int y)
        {
            this.x = (ushort)x;
            this.y = (ushort)y;
        }

        public static float2 operator +(float2 a, ushort2 b)
        {
            return a + (float2)b;
        }
        
        public static int2 operator *(ushort2 a, int b)
        {
            return new int2(a.x * b, a.y * b);
        }
        
        public static implicit operator int2(ushort2 vector)
        {
            return new int2(vector.x, vector.y);
        }

        public static implicit operator float2(ushort2 vector)
        {
            return new float2(vector.x, vector.y);
        }

        public static explicit operator ushort2(int2 vector)
        {
            return new ushort2(vector.x, vector.y);
        }

        public bool Equals(ushort2 other)
        {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj)
        {
            return obj is ushort2 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)math.hash(this);
        }
    }
}