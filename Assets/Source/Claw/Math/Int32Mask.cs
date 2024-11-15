using System.Runtime.CompilerServices;
using Beast.Claw.Input;

namespace Beast.Claw.Math
{
    public struct Int32Mask
    {
        public int value;
        
        public Int32Mask(int value)
        {
            this.value = value;
        }

        public static implicit operator Int32Mask(EInputAxis axisType)
        {
            return new Int32Mask((int)axisType);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Intersects(int mask)
        {
            return (value & mask) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Union(int mask)
        {
            value |= mask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool HitsButNotEqual(int mask)
        {
            return Intersects(mask) && value != mask;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }
}