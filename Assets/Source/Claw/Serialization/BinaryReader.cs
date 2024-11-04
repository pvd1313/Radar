using System;
using System.IO;
using Beast.Claw.Mathematics;
using UnityEngine.Profiling;

namespace Beast.Claw.Serialization
{
    public class BinaryReader : IBinaryReader
    {
        private readonly System.IO.BinaryReader _reader;
        private readonly Stream _stream;

        public BinaryReader(Stream stream)
        {
            _stream = stream;
            _reader = new System.IO.BinaryReader(_stream);
        }

        public float ReadFloat()
        {
            return _reader.ReadSingle();
        }

        public int ReadBytes(Span<byte> buffer)
        {
            Profiler.BeginSample("BinaryReader.ReadBytes()");
            
            const int BUFFER_LIMIT = 4096;

            int readTotal = 0;
            int lastReadCount = 1;

            while (readTotal < buffer.Length && lastReadCount > 0)
            {
                int bufferSize = BMath.Min(BUFFER_LIMIT, buffer.Length - readTotal);
                
                Span<byte> slice = buffer.Slice(readTotal, bufferSize);

                lastReadCount = _stream.Read(slice);

                readTotal += lastReadCount;
            }
            
            Profiler.EndSample();
            
            return readTotal;
        }

        public long ReadInt64()
        {
            return _reader.ReadInt64();
        }

        public ulong ReadUInt64()
        {
            return _reader.ReadUInt64();
        }

        public uint ReadUInt32()
        {
            return _reader.ReadUInt32();
        }

        public ushort ReadUInt16()
        {
            return _reader.ReadUInt16();
        }

        public short ReadInt16()
        {
            return _reader.ReadInt16();
        }

        public char ReadChar()
        {
            return _reader.ReadChar();
        }

        public string ReadString()
        {
            return _reader.ReadString();
        }

        public sbyte ReadSByte()
        {
            return _reader.ReadSByte();
        }

        public byte ReadByte()
        {
            return _reader.ReadByte();
        }

        public bool ReadBool()
        {
            return _reader.ReadBoolean();
        }
        
        public int ReadInt32()
        {
            return _reader.ReadInt32();
        }

        public void Dispose()
        {
            _reader?.Close();
            _reader?.Dispose();
        }
    }
}