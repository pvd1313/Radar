using System.Runtime.CompilerServices;

namespace Beast.Radar.Terrain
{
    public struct ByteArray16
    {
        private ulong _v0;
        private ulong _v1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetByte(int index)
        {
            if (index < 8)
            {
                return (byte)((_v0 >> (index * 8)) & 0xFF);
            }

            index -= 8;

            return (byte)((_v1 >> (index * 8)) & 0xFF);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetByte(int index, byte value)
        {
            if (index < 8)
            {
                ulong mask = 0xFFuL << (index * 8);
                _v0 = (_v0 & ~mask) | ((ulong)value << (index * 8));
            }
            else
            {
                index -= 8;
                ulong mask = 0xFFuL << (index * 8);
                _v1 = (_v1 & ~mask) | ((ulong)value << (index * 8));
            }
        }
    }
}