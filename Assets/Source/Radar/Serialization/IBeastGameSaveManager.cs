using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.Serialization
{
    public interface IBeastGameSaveManager
    {
        UniTask<BeastInitializationResult> LoadDefaultSave();

        UniTask<BeastInitializationResult> Load(string folderName, string fileName);
        
        UniTask<BeastInitializationResult> Save(string folderName, string fileName);
    }
}