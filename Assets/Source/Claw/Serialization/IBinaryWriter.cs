namespace Beast.Claw.Serialization
{
    public interface IBinaryWriter
    {
        void WriteFloat(float x);
        void WriteChar(char x);
        void WriteString(string x);
        void WriteUInt64(ulong x);
        void WriteUInt32(uint x);
        void WriteUInt16(ushort x);
        void WriteSByte(sbyte x);
        void WriteByte(byte x);
        void WriteInt16(short x);
        void WriteInt32(int x);
        void WriteInt64(long x);
    }
}