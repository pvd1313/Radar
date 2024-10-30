using System;
using System.IO;
using System.Threading.Tasks;

namespace Beast.Radar.Serialization
{
    public struct StreamData : IDisposable, IAsyncDisposable
    {
        public Stream stream;
        public string path;

        public void Dispose()
        {
            stream?.Close();
            stream?.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (stream != null) await stream.DisposeAsync();
        }
    }
}