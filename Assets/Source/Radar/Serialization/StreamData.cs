using System.IO;

namespace Beast.Radar.Serialization
{
    public struct StreamData
    {
        public Stream stream;
        public string path;
        public bool isReading;
    }
}