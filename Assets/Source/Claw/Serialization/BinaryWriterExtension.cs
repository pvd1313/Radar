using System;
using System.Runtime.Serialization;
using Beast.Claw.Collection;
using Unity.Mathematics;

namespace Beast.Claw.Serialization
{
    public static class BinaryWriterExtension
    {
        public static void Write<TData>(this IBinaryWriter stream, TData data) 
            where TData : ISerializable
        {
            data.Serialize(stream);
        }

        public static void Write<TData>(this IBinaryWriter stream, ref TData data)
            where TData : struct, ISerializable
        {
            data.Serialize(stream);
        }

        public static void WriteArray<TData>(this IBinaryWriter stream, TData[] array)
            where TData : ISerializable
        {
            if (array.IsNullOrEmpty())
            {
                stream.WriteInt32(0);
                return;
            }

            stream.WriteInt32(array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                array[i].Serialize(stream);
            }
        }

        public static void WriteGuid(this IBinaryWriter stream, Guid guid)
        {
            Span<byte> span = stackalloc byte[16];
            if (!guid.TryWriteBytes(span))
            {
                throw new SerializationException("Failed guid.TryWriteBytes(span)");
            }

            for (int i = 0; i < 16; i++)
            {
                stream.WriteByte(span[i]);
            }
        }

        public static void WriteQuaternion(this IBinaryWriter stream, quaternion quaternion)
        {
            float4 vector = quaternion.value;
            stream.WriteFloat(vector.x);
            stream.WriteFloat(vector.y);
            stream.WriteFloat(vector.z);
            stream.WriteFloat(vector.w);
        }
        
        public static void WriteFloat2(this IBinaryWriter stream, float2 vector)
        {
            stream.WriteFloat(vector.x);
            stream.WriteFloat(vector.y);
        }

        public static void WriteFloat3(this IBinaryWriter stream, float3 vector)
        {
            stream.WriteFloat(vector.x);
            stream.WriteFloat(vector.y);
            stream.WriteFloat(vector.z);
        }

        public static void WriteFloat4(this IBinaryWriter stream, float4 vector)
        {
            stream.WriteFloat(vector.x);
            stream.WriteFloat(vector.y);
            stream.WriteFloat(vector.z);
            stream.WriteFloat(vector.w);
        }

        public static void WriteInt2(this IBinaryWriter stream, int2 vector)
        {
            stream.WriteInt32(vector.x);
            stream.WriteInt32(vector.y);
        }

        public static void WriteInt3(this IBinaryWriter stream, int3 vector)
        {
            stream.WriteInt32(vector.x);
            stream.WriteInt32(vector.y);
            stream.WriteInt32(vector.z);
        }

        public static void WriteInt4(this IBinaryWriter stream, int4 vector)
        {
            stream.WriteInt32(vector.x);
            stream.WriteInt32(vector.y);
            stream.WriteInt32(vector.z);
            stream.WriteInt32(vector.w);
        }
    }
}