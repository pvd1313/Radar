namespace Beast.Claw.Serialization
{
    public interface IBeastBlobStreamWriter
    {
        void WriteJson<TOwner, TJson>(TOwner owner, string blobID, TJson data);

        void WriteBinary<TOwner, TData>(TOwner owner, string blobID, TData data)
            where TData : IBeastBinarySerializable;

        void WriteBinaryRef<TOwner, TData>(TOwner owner, string blobID, ref TData data)
            where TData : struct, IBeastBinarySerializable;
    }
}