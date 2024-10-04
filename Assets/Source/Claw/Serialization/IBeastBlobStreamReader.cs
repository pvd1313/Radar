namespace Beast.Claw.Serialization
{
    public interface IBeastBlobStreamReader
    {
        public bool TryReadJson<TOwner, TData>(TOwner owner, string blobID, out TData output);

        public bool TryReadBinaryStruct<TOwner, TDataReader>(TOwner owner, string blobID, out TDataReader reader)
            where TDataReader : struct, IBeastBinaryReader;
        public bool TryReadBinaryClass<TOwner, TDataReader>(TOwner owner, string blobID, TDataReader reader)
            where TDataReader : class, IBeastBinaryReader;
    }
}