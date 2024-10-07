using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.Serialization
{
    public interface IGameSaveManager
    {
        UniTask<OperationResult> LoadDefaultSave();

        UniTask<OperationResult> Load(string folderName, string fileName);
        
        UniTask<OperationResult> Save(string folderName, string fileName);
    }
}