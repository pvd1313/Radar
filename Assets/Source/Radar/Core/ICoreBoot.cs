using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.Core
{
    public interface ICoreBoot
    {
        public UniTask<ActionResult> Boot();
    }
}