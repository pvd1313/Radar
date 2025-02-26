using Unity.Mathematics;

namespace Beast.Claw.Math
{
    public struct sbyte2
    {
        public static readonly sbyte2 zero = new sbyte2(0, 0);
        
        public sbyte x;
        public sbyte y;

        public sbyte2(int x, int y)
        {
            this.x = (sbyte) x;
            this.y = (sbyte) y;
        }
        
        public static float2 operator+ (float2 a, sbyte2 b)
        {
            return new float2(a.x + b.x, a.y + b.y);
        }
        
        public static sbyte2 operator+ (sbyte2 a, sbyte2 b)
        {
            return new sbyte2(a.x + b.x, a.y + b.y);
        }
        
        public static implicit operator int2 (sbyte2 value)
        {
            return new int2(value.x, value.y);
        }

        public override string ToString()
        {
            return $"({x}, {y})";
        }
    }
}