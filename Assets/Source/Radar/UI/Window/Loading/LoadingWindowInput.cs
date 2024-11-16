using System;
using Beast.Claw.Common;
using Beast.Claw.ProgressTracker;

namespace Beast.Radar.UI
{
    public struct LoadingWindowInput
    {
        public IProgressTracker progressTracker;
        public float currentProgress;
        public bool clearLastProgress;
        public Action nextAction;
    }
}