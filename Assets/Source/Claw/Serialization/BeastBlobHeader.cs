using System.IO;

namespace Beast.Claw.Serialization
{
    public struct BeastBlobHeader
    {
        public int length;
        public string id;
        public EBeastBlobType type;

        public void ReadFromBinaryStream(BinaryReader stream)
        {
            id = stream.ReadString();
            type = (EBeastBlobType) stream.ReadByte();
            length = stream.ReadInt32();
        }

        public void WriteToBinaryStream(BinaryWriter stream)
        {
            stream.Write(id);
            stream.Write((byte)type);
            stream.Write(length);
        }
    }
}