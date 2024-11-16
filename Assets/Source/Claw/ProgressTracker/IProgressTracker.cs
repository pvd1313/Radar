using Beast.Claw.Common;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.ProgressTracker
{
    public interface IProgressTracker
    {
        float Progress { get; }
        
        void Begin(int stepCount, float overallProgressDelta);

        void SetListener(IProgressTrackerListener listener);
        
        UniTask Next(string stepName);
    }
}