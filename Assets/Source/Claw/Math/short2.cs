using System;
using Unity.Mathematics;

namespace Beast.Claw.Math
{
    public struct short2 : IEquatable<short2>
    {
        public short x;
        public short y;

        public short2(int x, int y)
        {
            this.x = (short)x;
            this.y = (short)y;
        }

        public static float2 operator +(float2 a, short2 b)
        {
            return a + (float2)b;
        }
        
        public static int2 operator *(short2 a, int b)
        {
            return new int2(a.x * b, a.y * b);
        }
        
        public static implicit operator int2(short2 vector)
        {
            return new int2(vector.x, vector.y);
        }

        public static implicit operator float2(short2 vector)
        {
            return new float2(vector.x, vector.y);
        }

        public static explicit operator short2(int2 vector)
        {
            return new short2(vector.x, vector.y);
        }

        public bool Equals(short2 other)
        {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj)
        {
            return obj is short2 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)math.hash(this);
        }
    }
}