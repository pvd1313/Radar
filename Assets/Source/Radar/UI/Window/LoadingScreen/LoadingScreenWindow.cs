using System;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Mathematics;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

namespace Beast.Radar.UI
{
    public class LoadingScreenWindow : IWindow<LoadingScreenInput>, IGameCycleListener
    {
        private const int MAX_TASK_COUNT = 20;

        private readonly EssentialPrefabLibrary _library;
        private readonly IWindowManager _windowManager;

        private LoadingScreenView _view;
        private UIPool<LoadingTaskText> _loadingTasksPool;

        private LoadingTaskText _lastLoadingTask;
        private IProgressTracker _progressTracker;
        private Action _progressButtonAction;

        public LoadingScreenWindow(EssentialPrefabLibrary library, IWindowManager windowManager)
        {
            _library = library;
            _windowManager = windowManager;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
        }
        
        public UniTask Open(LoadingScreenInput input)
        {
            _view.progressButton.SetInteractable(false);
            _view.gameObject.SetActive(true);

            _setProgressValue(input.currentProgress);
            _progressTracker = input.progressTracker;

            if (input.clearLastProgress)
            {
                _loadingTasksPool.DisableAll();
            }
            
            if (_progressTracker != null)
            {
                _progressTracker.Updated += _onProgressTrackerUpdated;
            }

            _progressButtonAction = input.continueButtonAction;
            
            return UniTask.CompletedTask;
        }
        
        public UniTask Close()
        {
            _view.gameObject.SetActive(false);

            if (_progressTracker != null)
            {
                _progressTracker.Updated -= _onProgressTrackerUpdated;
                _progressTracker = null;
            }
            
            return UniTask.CompletedTask;
        }

        private UniTask<ActionResult> _warmup(ConcurrentContext context)
        {
            _view = BScene.InstantiateMono(this, _library.loadingScreen, float3.zero, quaternion.identity);
            _view.gameObject.SetActive(false);

            _loadingTasksPool = new UIPool<LoadingTaskText>(_library.loadingTaskText, _view.logContent);
            
            _windowManager.Register(this, WindowID.LoadingScreen);
            _view.transform.SetParent(_windowManager.CanvasTransform, false);
            _view.progressButton.Clicked += _onProgressButtonClicked;
            
            return ActionResult.SuccessSyncronous();
        }
        
        private void _onProgressButtonClicked()
        {
            _progressButtonAction?.Invoke();
        }

        private void _onProgressTrackerUpdated(ProgressTrackerInfo info)
        {
            _setProgressValue(info.progress01);
            _addTask(info.currentStep);
            _stopLoadLastTask(string.Empty);
        }

        private void _addTask(string task)
        {
            if (_loadingTasksPool.GetEnabledCount() == MAX_TASK_COUNT)
            {
                _loadingTasksPool.Disable(_loadingTasksPool.GetByIndex(0));
            }

            _lastLoadingTask = _loadingTasksPool.GetNext();
            _lastLoadingTask.SetTask(task);
        }

        private void _stopLoadLastTask(string duration)
        {
            if (_lastLoadingTask != null)
            {
                _lastLoadingTask.Completed(duration);
                _lastLoadingTask = null;
            }
        }

        private void _setProgressValue(float value01)
        {
            _view.progressFill.SetValue(value01);
            _view.buttonText.SetText(BMath.FloorToInt(value01 * 100).ToString());
            
            if (value01 >= 1)
            {
                _view.progressButton.SetInteractable(true);
                _view.buttonText.SetText("TAP");
            }
        }
    }
}