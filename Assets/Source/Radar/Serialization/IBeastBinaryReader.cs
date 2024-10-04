using System.IO;

namespace Beast.Radar.Serialization
{
    public interface IBeastBinaryReader
    {
        void Read(BinaryReader stream);
    }
}