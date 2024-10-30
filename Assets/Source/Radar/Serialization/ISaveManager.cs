using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.Serialization
{
    public interface ISaveManager
    {
        UniTask<ActionResult> LoadAsync(GameModel model);

        bool Exists(string folderName, string fileName);

        UniTask<ActionResult> LoadAsync(string folderName, string fileName);
        
        void Save(string folderName, string fileName);
    }
}