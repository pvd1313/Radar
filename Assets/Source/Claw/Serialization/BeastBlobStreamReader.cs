using System;
using System.Collections.Generic;
using System.IO;
using Beast.Claw.Common;
using Beast.Claw.Serialization.Json;
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
        private readonly IBeastBinaryReader _binaryReader;
        private readonly JsonReader _jsonReader;
        private readonly JsonSerializer _jsonSerializer;
        private readonly Dictionary<string, Header> _headers;

        public BeastBlobStreamReader(Stream stream)
        {
            _stream = stream;
            _binaryReader = new BeastBinaryReader(_stream);
            _jsonReader = new JsonTextReader(new StreamReader(stream));
            _headers = new Dictionary<string, Header>();
            
            _jsonSerializer = new JsonSerializer();
            _jsonSerializer.Converters.AddRange(BeastJsonConverters.All);

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
                    BeastBlobHeader header = _binaryReader.ReadNew<BeastBlobHeader>();

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
                BDebug.LogException(owner, e, $"Failed to deserialize json blob '{blobID}'");

                return false;
            }

            return true;
        }

        public bool TryReadBinaryNew<TOwner, TData>(TOwner owner, string blobID, out TData data)
            where TData : IBeastBinarySerializable, new()
        {
            data = default;
            
            try
            {
                InternalBeginRead(blobID, EBeastBlobType.Binary);

                data = _binaryReader.ReadNew<TData>();
            }
            catch (Exception e)
            {
                BDebug.LogException(owner, e, $"Failed to deserialize binary blob '{blobID}'");
                
                return false;
            }

            return true;
        }
        
        public bool TryReadBinaryClass<TOwner, TData>(TOwner owner, string blobID, TData data)
            where TData : class, IBeastBinarySerializable
        {
            try
            {
                InternalBeginRead(blobID, EBeastBlobType.Binary);

                _binaryReader.ReadClass(data);
            }
            catch (Exception e)
            {
                BDebug.LogException(owner, e, $"Failed to deserialize binary blob '{blobID}'");
                
                return false;
            }

            return true;
        }

        private void InternalBeginRead(string blobID, EBeastBlobType blobType)
        {
            if (!_headers.TryGetValue(blobID, out Header header))
            {
                string message = $"'{blobID}' was not found in stream";
                throw new InvalidOperationException(message);
            }

            if (header.blob.type != blobType)
            {
                string message = $"'{blobID}' expected to be '{blobType}', but was '{header.blob.type}'";
                throw new InvalidOperationException(message);
            }

            if (header.blob.length <= 0)
            {
                string message = $"'{blobID}' has not correct length ({header.blob.length})";
                throw new InvalidOperationException(message);
            }
            
            _stream.Position = header.dataOffset;
        }
    }
}
