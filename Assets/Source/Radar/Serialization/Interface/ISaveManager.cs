using Beast.Claw.GameLoop;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.Serialization
{
    public interface ISaveManager
    {
        UniTask<EActionStatus> LoadAsync(GameModel model, ESaveManagerFlags flags);

        bool Exists(string folderName, string fileName);

        UniTask<EActionStatus> LoadAsync(string folderName, string fileName, ESaveManagerFlags flags);
        
        EActionStatus Save(string folderName, string fileName);
    }
}