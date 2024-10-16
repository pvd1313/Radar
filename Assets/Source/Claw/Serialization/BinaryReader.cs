using System.IO;

namespace Beast.Claw.Serialization
{
    public class BinaryReader : IBinaryReader
    {
        private readonly System.IO.BinaryReader _stream;

        public BinaryReader(Stream stream)
        {
            _stream = new System.IO.BinaryReader(stream);
        }

        public float ReadFloat()
        {
            return _stream.ReadSingle();
        }

        public long ReadInt64()
        {
            return _stream.ReadInt64();
        }

        public ulong ReadUInt64()
        {
            return _stream.ReadUInt64();
        }

        public uint ReadUInt32()
        {
            return _stream.ReadUInt32();
        }

        public ushort ReadUInt16()
        {
            return _stream.ReadUInt16();
        }

        public short ReadInt16()
        {
            return _stream.ReadInt16();
        }

        public char ReadChar()
        {
            return _stream.ReadChar();
        }

        public string ReadString()
        {
            return _stream.ReadString();
        }

        public sbyte ReadSByte()
        {
            return _stream.ReadSByte();
        }

        public byte ReadByte()
        {
            return _stream.ReadByte();
        }

        public bool ReadBool()
        {
            return _stream.ReadBoolean();
        }
        
        public int ReadInt32()
        {
            return _stream.ReadInt32();
        }
    }
}