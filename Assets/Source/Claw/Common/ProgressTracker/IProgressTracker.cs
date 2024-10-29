using System;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.Common
{
    public interface IProgressTracker
    {
        event Action<ProgressTrackerInfo> Updated;
        
        void Begin(int stepCount, float overallProgressDelta);
        UniTask Next(string stepName);
        float Progress { get; }
    }
}