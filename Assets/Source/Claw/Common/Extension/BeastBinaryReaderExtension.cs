using Beast.Claw.Serialization;
using Unity.Mathematics;

namespace Beast.Claw.Common
{
    public static class BeastBinaryReaderExtension
    {
        public static TData ReadNew<TData>(this IBeastBinaryReader stream) 
            where TData : IBeastBinarySerializable, new()
        {
            TData data = new TData();

            data.Deserialize(stream);

            return data;
        }

        public static void ReadClass<TData>(this IBeastBinaryReader stream, TData data) 
            where TData : class, IBeastBinarySerializable
        {
            data.Deserialize(stream);
        }

        public static void ReadRef<TData>(this IBeastBinaryReader stream, ref TData data) 
            where TData : struct, IBeastBinarySerializable
        {
            data.Deserialize(stream);
        }

        public static quaternion ReadQuaternion(this IBeastBinaryReader stream)
        {
            float x = stream.ReadFloat();
            float y = stream.ReadFloat();
            float z = stream.ReadFloat();
            float w = stream.ReadFloat();

            return new quaternion(x, y, z, w);
        }
        
        public static float2 ReadFloat2(this IBeastBinaryReader stream)
        {
            float x = stream.ReadFloat();
            float y = stream.ReadFloat();

            return new float2(x, y);
        }

        public static float3 ReadFloat3(this IBeastBinaryReader stream)
        {
            float x = stream.ReadFloat();
            float y = stream.ReadFloat();
            float z = stream.ReadFloat();

            return new float3(x, y, z);
        }

        public static float4 ReadFloat4(this IBeastBinaryReader stream)
        {
            float x = stream.ReadFloat();
            float y = stream.ReadFloat();
            float z = stream.ReadFloat();
            float w = stream.ReadFloat();

            return new float4(x, y, z, w);
        }
        
        public static int2 ReadInt2(this IBeastBinaryReader stream)
        {
            int x = stream.ReadInt32();
            int y = stream.ReadInt32();

            return new int2(x, y);
        }

        public static int3 ReadInt3(this IBeastBinaryReader stream)
        {
            int x = stream.ReadInt32();
            int y = stream.ReadInt32();
            int z = stream.ReadInt32();

            return new int3(x, y, z);
        }
        
        public static int4 ReadInt4(this IBeastBinaryReader stream)
        {
            int x = stream.ReadInt32();
            int y = stream.ReadInt32();
            int z = stream.ReadInt32();
            int w = stream.ReadInt32();

            return new int4(x, y, z, w);
        }
    }
}