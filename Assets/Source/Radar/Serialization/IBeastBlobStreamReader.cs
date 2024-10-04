namespace Beast.Radar.Serialization
{
    public interface IBeastBlobStreamReader
    {
        public bool TryReadJson<TOwner, TData>(TOwner owner, string blobID, out TData output);

        public bool TryReadBinary<TOwner, TDataReader>(TOwner owner, string blobID, TDataReader reader)
            where TDataReader : IBeastBinaryReader;
    }
}