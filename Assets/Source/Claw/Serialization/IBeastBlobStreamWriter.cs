namespace Beast.Claw.Serialization
{
    public interface IBeastBlobStreamWriter
    {
        public void WriteJson<TOwner, TJson>(TOwner owner, string blobID, TJson data);

        public void WriteBinary<TOwner, TWriter>(TOwner owner, string blobID, TWriter writer)
            where TWriter : IBeastBinaryWriter;
    }
}