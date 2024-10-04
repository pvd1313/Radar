namespace Beast.Claw.Serialization
{
    public struct BeastBlobHeader : IBeastBinarySerializable
    {
        public int length;
        public string id;
        public EBeastBlobType type;

        public void Serialize(IBeastBinaryWriter stream)
        {
            stream.WriteString(id);
            stream.WriteByte((byte)type);
            stream.WriteInt32(length);
        }

        public void Deserialize(IBeastBinaryReader stream)
        {
            id = stream.ReadString();
            type = (EBeastBlobType) stream.ReadByte();
            length = stream.ReadInt32();
        }
    }
}