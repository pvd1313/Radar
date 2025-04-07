using System;
using System.IO;
using System.Threading.Tasks;
using Beast.Claw.Debug;
using Beast.Claw.Math;

namespace Beast.Claw.Serialization
{
    public class BinaryWriter : IBinaryWriter, IAsyncDisposable
    {
        private readonly System.IO.BinaryWriter _writer;
        private readonly Stream _stream;
        
        public BinaryWriter(Stream stream)
        {
            _stream = stream;
            _writer = new System.IO.BinaryWriter(_stream);
        }

        public void WriteBytes(Span<byte> buffer)
        {
            BProfiler.BeginSample("BinaryWriter.WriteBytes()");
            
            const int BUFFER_LIMIT = 4096;

            int wroteCount = 0;
            
            while (wroteCount < buffer.Length)
            {
                int bufferSize = BMath.Min(BUFFER_LIMIT, buffer.Length - wroteCount);

                Span<byte> slice = buffer.Slice(wroteCount, bufferSize);

                _stream.Write(slice);
                
                _stream.Flush();

                wroteCount += bufferSize;
            }
            
            BProfiler.EndSample();
        }
        
        public void WriteFloat(float x)
        {
            _writer.Write(x);
        }

        public void WriteChar(char x)
        {
            _writer.Write(x);
        }

        public void WriteString(string x)
        {
            _writer.Write(x);
        }

        public void WriteUInt64(ulong x)
        {
            _writer.Write(x);
        }

        public void WriteUInt32(uint x)
        {
            _writer.Write(x);
        }

        public void WriteUInt16(ushort x)
        {
            _writer.Write(x);
        }

        public void WriteSByte(sbyte x)
        {
            _writer.Write(x);
        }

        public void WriteByte(byte x)
        {
            _writer.Write(x);
        }

        public void WriteInt16(short x)
        {
            _writer.Write(x);
        }

        public void WriteInt32(int x)
        {
            _writer.Write(x);
        }

        public void WriteInt64(long x)
        {
            _writer.Write(x);
        }

        public void WriteBool(bool x)
        {
            _writer.Write(x);
        }

        public void Dispose()
        {
            _writer.Flush();
            _writer.Close();
            _writer.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (_writer != null) await _writer.DisposeAsync();
        }
    }
}