namespace Beast.Claw.Serialization
{
    public interface IBeastBlobStreamReader
    {
        bool TryReadJson<TOwner, TData>(TOwner owner, string blobID, out TData output);

        bool TryReadBinaryNew<TOwner, TData>(TOwner owner, string blobID, out TData data)
            where TData : IBeastBinarySerializable, new();

        bool TryReadBinaryClass<TOwner, TData>(TOwner owner, string blobID, TData data)
            where TData : class, IBeastBinarySerializable;
    }
}