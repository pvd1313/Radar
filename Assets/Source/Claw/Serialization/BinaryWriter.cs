using System.IO;

namespace Beast.Claw.Serialization
{
    public class BinaryWriter : IBinaryWriter
    {
        private readonly System.IO.BinaryWriter _stream;
        
        public BinaryWriter(Stream stream)
        {
            _stream = new System.IO.BinaryWriter(stream);
        }

        public void WriteFloat(float x)
        {
            _stream.Write(x);
        }

        public void WriteChar(char x)
        {
            _stream.Write(x);
        }

        public void WriteString(string x)
        {
            _stream.Write(x);
        }

        public void WriteUInt64(ulong x)
        {
            _stream.Write(x);
        }

        public void WriteUInt32(uint x)
        {
            _stream.Write(x);
        }

        public void WriteUInt16(ushort x)
        {
            _stream.Write(x);
        }

        public void WriteSByte(sbyte x)
        {
            _stream.Write(x);
        }

        public void WriteByte(byte x)
        {
            _stream.Write(x);
        }

        public void WriteInt16(short x)
        {
            _stream.Write(x);
        }

        public void WriteInt32(int x)
        {
            _stream.Write(x);
        }

        public void WriteInt64(long x)
        {
            _stream.Write(x);
        }
    }
}