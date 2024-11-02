using System.Runtime.CompilerServices;

namespace Beast.Radar.Terrain
{
    public struct ByteArray16
    {
        public ulong v0;
        public ulong v1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetByte(int index)
        {
            if (index < 8)
            {
                return (byte)((v0 >> (index * 8)) & 0xFF);
            }

            index -= 8;

            return (byte)((v1 >> (index * 8)) & 0xFF);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetByte(int index, byte value)
        {
            if (index < 8)
            {
                ulong mask = 0xFFuL << (index * 8);
                v0 = (v0 & ~mask) | ((ulong)value << (index * 8));
            }
            else
            {
                index -= 8;
                ulong mask = 0xFFuL << (index * 8);
                v1 = (v1 & ~mask) | ((ulong)value << (index * 8));
            }
        }
    }
}