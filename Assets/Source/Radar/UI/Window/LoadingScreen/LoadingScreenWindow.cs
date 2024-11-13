using System;
using System.Text;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Mathematics;
using Beast.Claw.UI;
using Cysharp.Threading.Tasks;
using Noesis;

namespace Beast.Radar.UI
{
    public class LoadingScreenWindow : Controller, IWindow<LoadingScreenInput>, IGameCycleListener, IProgressTrackerListener
    {
        private readonly EssentialPrefabLibrary _library;
        private readonly LoadingScreenConfig _config;
        private readonly INoesisWindowManager _windows;
        private readonly IWindowManager _legacy;
        
        private LoadingScreenView _view;
        
        private IProgressTracker _progressTracker;
        private IntToStringConverter _intToString;
        private Action _nextAction;
        private StringBuilder _stringBuilder;
        private bool _canInvokeNextAction;

        public LoadingScreenWindow(
            LoadingScreenConfig config, 
            INoesisWindowManager windows, 
            IWindowManager legacy)
        {
            _config = config;
            _windows = windows;
            _legacy = legacy;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
        }
        
        public UniTask Open(LoadingScreenInput input)
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
        
        private UniTask _warmup(ConcurrentContext context)
        {
            _intToString = new IntToStringConverter(99);
            _stringBuilder = new StringBuilder();
            
            _config.loadingScreenView.Instantiate(out _view);
            
            _view.Visibility = Visibility.Collapsed;
            _view.grid.Background = new ImageBrush(new TextureSource(_config.backgroundTexture));
            
            // TODO: #R72 This is not ideal, as we have to deal with two events. Will be fixed when input is migrated.
            _view.MouseDown += (_, _) =>
            {
                if (_canInvokeNextAction)
                {
                    _nextAction?.Invoke();
                }
            };
            
            _view.TouchDown += (_, _) =>
            {
                if (_canInvokeNextAction)
                {
                    _nextAction?.Invoke();
                }
            };
            
            _windows.Attach(_view);
            
            _legacy.Register(this, WindowID.LoadingScreen);
            
            return UniTask.CompletedTask;
        }
    }
}