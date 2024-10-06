using Beast.Claw.Serialization;
using Unity.Mathematics;

namespace Beast.Claw.Common
{
    public static class BeastBinaryWriterExtension
    {
        public static void Write<TData>(this IBeastBinaryWriter stream, TData data) 
            where TData : IBeastSerializable
        {
            data.Serialize(stream);
        }

        public static void Write<TData>(this IBeastBinaryWriter stream, ref TData data)
            where TData : struct, IBeastSerializable
        {
            data.Serialize(stream);
        }

        public static void WriteQuaternion(this IBeastBinaryWriter stream, quaternion quaternion)
        {
            float4 vector = quaternion.value;
            stream.WriteFloat(vector.x);
            stream.WriteFloat(vector.y);
            stream.WriteFloat(vector.z);
            stream.WriteFloat(vector.w);
        }
        
        public static void WriteFloat2(this IBeastBinaryWriter stream, float2 vector)
        {
            stream.WriteFloat(vector.x);
            stream.WriteFloat(vector.y);
        }

        public static void WriteFloat3(this IBeastBinaryWriter stream, float3 vector)
        {
            stream.WriteFloat(vector.x);
            stream.WriteFloat(vector.y);
            stream.WriteFloat(vector.z);
        }

        public static void WriteFloat4(this IBeastBinaryWriter stream, float4 vector)
        {
            stream.WriteFloat(vector.x);
            stream.WriteFloat(vector.y);
            stream.WriteFloat(vector.z);
            stream.WriteFloat(vector.w);
        }

        public static void WriteInt2(this IBeastBinaryWriter stream, int2 vector)
        {
            stream.WriteInt32(vector.x);
            stream.WriteInt32(vector.y);
        }

        public static void WriteInt3(this IBeastBinaryWriter stream, int3 vector)
        {
            stream.WriteInt32(vector.x);
            stream.WriteInt32(vector.y);
            stream.WriteInt32(vector.z);
        }

        public static void WriteInt4(this IBeastBinaryWriter stream, int4 vector)
        {
            stream.WriteInt32(vector.x);
            stream.WriteInt32(vector.y);
            stream.WriteInt32(vector.z);
            stream.WriteInt32(vector.w);
        }
    }
}