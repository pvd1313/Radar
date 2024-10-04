using System;
using System.Collections.Generic;
using System.IO;
using Beast.Claw.Common;
using Newtonsoft.Json;

namespace Beast.Claw.Serialization
{
    public class BeastBlobStreamReader : IBeastBlobStreamReader
    {
        private struct Header
        {
            public BeastBlobHeader blob;
            public long dataOffset;
        }

        private readonly Stream _stream;
        private readonly BinaryReader _binaryReader;
        private readonly JsonReader _jsonReader;
        private readonly JsonSerializer _jsonSerializer;
        private readonly Dictionary<string, Header> _headers;

        public BeastBlobStreamReader(Stream stream)
        {
            _stream = stream;
            _binaryReader = new BinaryReader(_stream);
            _jsonReader = new JsonTextReader(new StreamReader(stream));
            _jsonSerializer = new JsonSerializer();
            _headers = new Dictionary<string, Header>();

            if (!stream.CanSeek)
            {
                throw new InvalidOperationException("Stream must support seeking");
            }
        }

        public void ReadHeaders()
        {
            _headers.Clear();
            
            while (_stream.Position < _stream.Length)
            {
                try
                {
                    BeastBlobHeader header = new();
                    header.ReadFromBinaryStream(_binaryReader);

                    _headers.Add(header.id, new Header
                    {
                        blob = header,
                        dataOffset = _stream.Position,
                    });

                    _stream.Position += header.length;
                }
                catch (EndOfStreamException)
                {
                    
                }
            }

            _stream.Position = 0;
        }
        
        public bool TryReadJson<TOwner, TJson>(TOwner owner, string blobID, out TJson output)
        {
            output = default;
            
            try
            {
                InternalBeginRead(blobID, EBeastBlobType.Json);

                output = _jsonSerializer.Deserialize<TJson>(_jsonReader);
            }
            catch (Exception e)
            {
                BLog.Exception(owner, e, $"Failed to deserialize json blob '{blobID}'");

                return false;
            }

            return true;
        }

        public bool TryReadBinaryStruct<TOwner, TDataReader>(TOwner owner, string blobID, out TDataReader reader)
            where TDataReader : struct, IBeastBinaryReader
        {
            reader = new TDataReader();
            
            try
            {
                InternalBeginRead(blobID, EBeastBlobType.Binary);
                
                reader.BinaryDeserialize(_binaryReader);
            }
            catch (Exception e)
            {
                BLog.Exception(owner, e, $"Failed to deserialize binary blob '{blobID}'");
                
                return false;
            }

            return true;
        }
        
        public bool TryReadBinaryClass<TOwner, TDataReader>(TOwner owner, string blobID, TDataReader reader)
            where TDataReader : class, IBeastBinaryReader
        {
            try
            {
                InternalBeginRead(blobID, EBeastBlobType.Binary);
                
                reader.BinaryDeserialize(_binaryReader);
            }
            catch (Exception e)
            {
                BLog.Exception(owner, e, $"Failed to deserialize binary blob '{blobID}'");
                
                return false;
            }

            return true;
        }

        private void InternalBeginRead(string blobID, EBeastBlobType blobType)
        {
            if (!_headers.TryGetValue(blobID, out Header header))
            {
                throw new InvalidOperationException($"'{blobID}' was not found in stream");
            }

            if (header.blob.type != blobType)
            {
                throw new InvalidOperationException($"'{blobID}' expected to be '{blobType}', but was '{header.blob.type}'");
            }
            
            _stream.Position = header.dataOffset;
        }
    }
}
