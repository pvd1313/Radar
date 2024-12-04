using System;
using System.Text;
using Beast.Claw.Attributes;
using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Claw.Math;
using Beast.Claw.ProgressTracker;
using Beast.Claw.UI;
using Cysharp.Threading.Tasks;
using Noesis;

namespace Beast.Radar.UI
{
    public class LoadingWindow : Actor, IWindow<LoadingWindowInput>, IProgressTrackerListener
    {
        [Required] private readonly EssentialPrefabLibrary _library;
        [Required] private readonly LoadingWindowConfig _config;
        [Required] private readonly IWindowManager _windows;
        
        private LoadingWindowView _view;
        
        private IProgressTracker _progressTracker;
        private IntToStringConverter _intToString;
        private Action _nextAction;
        private StringBuilder _stringBuilder;
        private bool _canInvokeNextAction;

        protected override UniTask _warmup(ConcurrentContext context)
        {
            _intToString = new IntToStringConverter(0, 99);
            _stringBuilder = new StringBuilder();
            
            _config.asset.Instantiate(out _view);
            
            _view.Visibility = Visibility.Collapsed;
            
            // Crashes Vulkan. Waiting for fix.
            // ImageBrush brush = new (new TextureSource(_config.backgroundTexture));
            // brush.Stretch = Stretch.UniformToFill;
            // _view.grid.Background = brush;
            
            _view.TouchDown += (_, _) =>
            {
                if (_canInvokeNextAction)
                {
                    _nextAction?.Invoke();
                }
            };
            
            _windows.Attach(_view, 1);
            
            _windows.Register(this, WindowID.LOADING);
            
            return UniTask.CompletedTask;
        }
        
        public UniTask Open(LoadingWindowInput input)
        {
            _view.Visibility = Visibility.Visible;

            input.progressTracker?.SetListener(this);

            if (input.clearLastProgress)
            {
                _stringBuilder.Clear();
            }

            _updateDisplay(input.currentProgress);

            _canInvokeNextAction = input.currentProgress >= 1;
            _nextAction = input.nextAction;
            
            return UniTask.CompletedTask;
        }
        
        public UniTask Close()
        {
            _view.Visibility = Visibility.Collapsed;
            
            return UniTask.CompletedTask;
        }
        
        public void UpdateProgress(ProgressTrackerInfo info)
        {
            _stringBuilder.Append(info.currentStepName);
            _stringBuilder.Append("...");
            _stringBuilder.AppendLine();
            
            if (info.updateDisplay)
            {
                _updateDisplay(info.progress01);
            }

            _canInvokeNextAction = info.progress01 >= 1;
        }

        private void _updateDisplay(float progress01)
        {
            _view.progressBar.Value = progress01;
            
            int progress100 = BMath.FloorToInt(progress01 * 100);
                
            _view.progressText.Text = progress100 < 100 ? _intToString.Convert(progress100) : "TAP";

            _view.textLog.Text = _stringBuilder.ToString();
        }
    }
}