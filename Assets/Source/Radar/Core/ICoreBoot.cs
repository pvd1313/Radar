using Beast.Claw.Common;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.Core
{
    public interface ICoreBoot
    {
        public UniTask Boot(IProgressTracker tracker);
    }
}