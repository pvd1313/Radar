using System;
using System.Collections.Generic;
using System.IO;
using Beast.Claw.Common;
using Beast.Claw.Reflection;
using Newtonsoft.Json;

namespace Beast.Claw.Serialization
{
    public class BeastBlobStreamWriter : IBeastBlobStreamWriter
    {
        private readonly Stream _stream;
        private readonly Dictionary<string, string> _blobID2Owner;
        private readonly BinaryWriter _binaryWriter;
        private readonly JsonWriter _jsonWriter;
        private readonly JsonSerializer _jsonSerializer;
        
        private long _writeBeginPosition;
        private long _blobHeaderSize;

        public BeastBlobStreamWriter(Stream stream)
        {
            _stream = stream;
            _binaryWriter = new BinaryWriter(_stream);
            _jsonWriter = new JsonTextWriter(new StreamWriter(_stream));
            _jsonSerializer = new JsonSerializer();
            _blobID2Owner = new Dictionary<string, string>();
            
            if (!stream.CanSeek)
            {
                throw new InvalidOperationException("Stream must support seeking");
            }
        }
        
        public void WriteJson<TOwner, TJson>(TOwner owner, string blobID, TJson data)
        {
            try
            {
                InternalBeginWrite(blobID, EBeastBlobType.Json);
                
                _blobID2Owner[blobID] = BeastTypeStorage<TOwner>.FullName;

                try
                {
                    _jsonSerializer.Serialize(_jsonWriter, data);
                }
                catch (Exception e)
                {
                    BLog.Exception(owner, e, $"Failed to serialize json blob '{blobID}' #B");
                }
                
                InternalEndWrite(blobID, EBeastBlobType.Json);
            }
            catch (Exception e)
            {
                BLog.Exception(owner, e, $"Failed to serialize json blob '{blobID}' #A");
            }
        }

        public void WriteBinary<TOwner, TWriter>(TOwner owner, string blobID, TWriter writer) 
            where TWriter : IBeastBinaryWriter
        {
            try
            {
                InternalBeginWrite(blobID, EBeastBlobType.Binary);
                
                _blobID2Owner[blobID] = BeastTypeStorage<TOwner>.FullName;

                try
                {
                    writer.BinarySerialize(_binaryWriter);
                }
                catch (Exception e)
                {
                    BLog.Exception(owner, e, $"Failed to serialize binary blob '{blobID}' #B");
                }
                
                InternalEndWrite(blobID, EBeastBlobType.Binary);
            }
            catch (Exception e)
            {
                BLog.Exception(owner, e, $"Failed to serialize binary blob '{blobID}' #A");
            }
        }

        private void InternalBeginWrite(string blobID, EBeastBlobType blobType)
        {
            if (_blobID2Owner.TryGetValue(blobID, out string lastOwner))
            {
                throw new InvalidOperationException($"Blob '{blobID}' is already occupied by '{lastOwner}'");
            }

            _writeBeginPosition = _stream.Position;

            BeastBlobHeader header = new ()
            {
                id = blobID,
                type = blobType,
                length = 0
            };
            
            header.WriteToBinaryStream(_binaryWriter);
            
            _blobHeaderSize = _stream.Position - _writeBeginPosition;
        }

        private void InternalEndWrite(string blobID, EBeastBlobType blobType)
        {
            long streamPositionBackup = _stream.Position;
            
            int blobSize = (int)(streamPositionBackup - _writeBeginPosition - _blobHeaderSize);

            _stream.Position = _writeBeginPosition;

            BeastBlobHeader header = new()
            {
                id = blobID,
                type = blobType,
                length = blobSize
            };

            header.WriteToBinaryStream(_binaryWriter);

            _stream.Position = streamPositionBackup;
        }
    }
}