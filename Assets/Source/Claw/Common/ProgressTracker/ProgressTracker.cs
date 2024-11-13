using System;
using System.Diagnostics;
using Beast.Claw.Mathematics;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.Common
{
    public class ProgressTracker : IProgressTracker
    {
        private const int FRAME_UPDATE_TRIGGER_MS = 16;

        public float Progress => _progressMax;

        private readonly Stopwatch _timer;
        
        private float _progressMin;
        private float _progressMax;
        private int _stageStepCount;
        private int _currentStep;
        private string _currentStepName;
        private IProgressTrackerListener _listener;

        public ProgressTracker()
        {
            _timer = new Stopwatch();
            _timer.Start();
        }

        public void Begin(int stepCount, float overallProgressDelta)
        {
            _progressMin = _progressMax;
            _progressMax = BMath.Clamp01(_progressMax + overallProgressDelta);
            _stageStepCount = stepCount;
            _currentStep = 0;
            _timer.Restart();
        }

        public void SetListener(IProgressTrackerListener listener)
        {
            _listener = listener;
        }

        public async UniTask Next(string stepName)
        {
            bool delayFrame = false;
            
            if (_timer.ElapsedMilliseconds > FRAME_UPDATE_TRIGGER_MS || _currentStep == 0)
            {
                delayFrame = true;
                
                _timer.Restart();
            }
            
            _listener?.UpdateProgress(new ProgressTrackerInfo
            {
                currentStepName = stepName,
                progress01 = BMath.Lerp(_progressMin, _progressMax, _currentStep / (float)_stageStepCount),
                updateDisplay = delayFrame
            });

            if (delayFrame)
            {
                await UniTask.DelayFrame(1);
            }
            
            _currentStep++;
        }
    }
}