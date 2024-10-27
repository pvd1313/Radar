using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.Serialization
{
    public interface ISaveManager
    {
        UniTask<ActionResult> Load(GameModel model);

        UniTask<bool> Exists(string folderName, string fileName);

        UniTask<ActionResult> Load(string folderName, string fileName);
        
        UniTask<ActionResult> Save(string folderName, string fileName);
    }
}