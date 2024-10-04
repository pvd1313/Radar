using System.IO;

namespace Beast.Claw.Serialization
{
    public interface IBeastBinaryWriter
    {
        void BinarySerialize(BinaryWriter stream);
    }
}