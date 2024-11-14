using System;
using Beast.Claw.Common;

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