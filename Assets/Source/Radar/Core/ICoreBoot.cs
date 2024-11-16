using Beast.Claw.Common;
using Beast.Claw.ProgressTracker;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.Core
{
    public interface ICoreBoot
    {
        public UniTask Boot(IProgressTracker tracker);
    }
}