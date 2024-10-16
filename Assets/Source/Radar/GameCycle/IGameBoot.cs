using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.GameCycle
{
    public interface IGameBoot
    {
        public UniTask<OperationResult> Boot();
    }
}