using System.IO;

namespace Beast.Radar.Serialization
{
    public interface IBeastBinaryWriter
    {
        void Write(BinaryWriter stream);
    }
}