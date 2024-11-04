using System;

namespace Beast.Claw.Serialization
{
    public interface IBinaryReader : IDisposable
    {
        float ReadFloat();
        long ReadInt64();
        ulong ReadUInt64();
        uint ReadUInt32();
        ushort ReadUInt16();
        short ReadInt16();
        char ReadChar();
        string ReadString();
        sbyte ReadSByte();
        byte ReadByte();
        int ReadInt32();
        bool ReadBool();
        int ReadBytes(Span<byte> buffer);
    }
}