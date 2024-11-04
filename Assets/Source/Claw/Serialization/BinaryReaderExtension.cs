using System;
using System.Runtime.Serialization;
using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Claw.Serialization
{
    public static class BinaryReaderExtension
    {
        public static TData Read<TData>(this IBinaryReader stream) 
            where TData : ISerializable, new()
        {
            TData data = new TData();

            data.Deserialize(stream);

            return data;
        }

        public static void Read<TData>(this IBinaryReader stream, TData data) 
            where TData : class, ISerializable
        {
            data.Deserialize(stream);
        }

        public static void Read<TData>(this IBinaryReader stream, ref TData data) 
            where TData : struct, ISerializable
        {
            data.Deserialize(stream);
        }

        public static TData[] ReadArrayAndLength<TData>(this IBinaryReader stream) 
            where TData : ISerializable, new()
        {
            int itemCount = stream.ReadInt32();
            if (itemCount == 0)
            {
                return Array.Empty<TData>();
            }

            TData[] array = new TData[itemCount];

            for (int i = 0; i < itemCount; i++)
            {
                TData data = new();
                
                data.Deserialize(stream);
                
                array[i] = data;
            }

            return array;
        }

        public static NativeArray<byte> ReadNativeBytesAndLength(this IBinaryReader stream, Allocator allocator)
        {
            int byteCount = stream.ReadInt32();
            if (byteCount == 0)
            {
                return new NativeArray<byte>(0, allocator);
            }

            NativeArray<byte> buffer = new (byteCount, allocator);
            
            int readCount = stream.ReadBytes(buffer.AsSpan());
            
            if (readCount != byteCount)
            {
                string message = $"Read byte count ({readCount}) mismatches expected byte count ({byteCount})";
                throw new SerializationException(message);
            }

            return buffer;
        }

        public static quaternion ReadQuaternion(this IBinaryReader stream)
        {
            float x = stream.ReadFloat();
            float y = stream.ReadFloat();
            float z = stream.ReadFloat();
            float w = stream.ReadFloat();

            return new quaternion(x, y, z, w);
        }

        public static Guid ReadGuid(this IBinaryReader stream)
        {
            Span<byte> span = stackalloc byte[16];
            for (int i = 0; i < 16; i++)
            {
                span[i] = stream.ReadByte();
            }

            return new Guid(span);
        }
        
        public static float2 ReadFloat2(this IBinaryReader stream)
        {
            float x = stream.ReadFloat();
            float y = stream.ReadFloat();

            return new float2(x, y);
        }

        public static float3 ReadFloat3(this IBinaryReader stream)
        {
            float x = stream.ReadFloat();
            float y = stream.ReadFloat();
            float z = stream.ReadFloat();

            return new float3(x, y, z);
        }

        public static float4 ReadFloat4(this IBinaryReader stream)
        {
            float x = stream.ReadFloat();
            float y = stream.ReadFloat();
            float z = stream.ReadFloat();
            float w = stream.ReadFloat();

            return new float4(x, y, z, w);
        }
        
        public static int2 ReadInt2(this IBinaryReader stream)
        {
            int x = stream.ReadInt32();
            int y = stream.ReadInt32();

            return new int2(x, y);
        }

        public static int3 ReadInt3(this IBinaryReader stream)
        {
            int x = stream.ReadInt32();
            int y = stream.ReadInt32();
            int z = stream.ReadInt32();

            return new int3(x, y, z);
        }
        
        public static int4 ReadInt4(this IBinaryReader stream)
        {
            int x = stream.ReadInt32();
            int y = stream.ReadInt32();
            int z = stream.ReadInt32();
            int w = stream.ReadInt32();

            return new int4(x, y, z, w);
        }
    }
}