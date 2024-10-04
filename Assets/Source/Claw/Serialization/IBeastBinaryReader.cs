using System.IO;

namespace Beast.Claw.Serialization
{
    public interface IBeastBinaryReader
    {
        void BinaryDeserialize(BinaryReader stream);
    }
}