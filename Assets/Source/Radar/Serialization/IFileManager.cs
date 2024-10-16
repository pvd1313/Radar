using Cysharp.Threading.Tasks;

namespace Beast.Radar.Serialization
{
    public interface IFileManager
    {
        UniTask<bool> Exists(string folderName, string fileName);
        UniTask<StreamData> OpenToRead(string folderName, string fileName);
        UniTask<StreamData> OpenToWrite(string folderName, string fileName);
        UniTask Close(StreamData stream);
    }
}