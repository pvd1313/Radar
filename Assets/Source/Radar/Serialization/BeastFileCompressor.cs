using System.IO;
using System.IO.Compression;

namespace Beast.Radar.Serialization
{
    public class BeastFileCompressor
    {
        private readonly byte[] _buffer;

        public BeastFileCompressor()
        {
            _buffer = new byte[4096];
        }
        
        public void Compress(string sourceFilePath, string targetFilePath)
        {
            using FileStream sourceStream = new (sourceFilePath, FileMode.Open);
            using FileStream targetStream = new (targetFilePath, FileMode.Create);
            GZipStream targetZipStream = new (targetStream, CompressionLevel.Fastest);

            while (sourceStream.Position < sourceStream.Length)
            {
                int byteCount = sourceStream.Read(_buffer);
                targetZipStream.Write(_buffer, 0, byteCount);
            }
            
            sourceStream.Close();
            
            targetZipStream.Flush();
            targetZipStream.Close();
        }

        public void Decompress(string sourceFilePath, string targetFilePath)
        {
            using FileStream sourceStream = new (sourceFilePath, FileMode.Open);
            using FileStream targetStream = new (targetFilePath, FileMode.Create);
            GZipStream sourceZipStream = new (sourceStream, CompressionMode.Decompress);

            while (sourceStream.Position < sourceStream.Length)
            {
                int byteCount = sourceZipStream.Read(_buffer);
                targetStream.Write(_buffer, 0, byteCount);
            }
            
            sourceZipStream.Close();

            targetStream.Flush();
            targetStream.Close();
        }
    }
}