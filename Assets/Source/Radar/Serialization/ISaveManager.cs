using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.Serialization
{
    public interface ISaveManager
    {
        UniTask<OperationResult> Load(GameModel model);

        UniTask<bool> Exists(string folderName, string fileName);

        UniTask<OperationResult> Load(string folderName, string fileName);
        
        UniTask<OperationResult> Save(string folderName, string fileName);
    }
}