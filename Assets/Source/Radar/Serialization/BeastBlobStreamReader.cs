using System;
using System.Collections.Generic;
using System.IO;
using Beast.Claw.Common;
using Newtonsoft.Json;

namespace Beast.Radar.Serialization
{
    public class BeastBlobStreamReader : IBeastBlobStreamReader
    {
        private struct Header
        {
            public BeastBlobHeader header;
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
                        header = header,
                        dataOffset = _stream.Position
                    });
                }
                catch (EndOfStreamException)
                {
                    
                }
            }
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

        public bool TryReadBinary<TOwner, TDataReader>(TOwner owner, string blobID, TDataReader reader)
            where TDataReader : IBeastBinaryReader
        {
            try
            {
                InternalBeginRead(blobID, EBeastBlobType.Binary);
                
                reader.Read(_binaryReader);
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

            if (header.header.type != EBeastBlobType.Binary)
            {
                throw new InvalidOperationException($"'{blobID}' expected to be '{blobType}', but was '{header.header.type}'");
            }
            
            _stream.Position = header.dataOffset;
        }
    }
}
