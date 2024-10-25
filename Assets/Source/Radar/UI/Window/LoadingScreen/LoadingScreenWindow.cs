using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public class LoadingScreenWindow : IWindow, IGameCycleListener
    {
        private const  int MAX_TASKS_COUNT = 20;
        
        private readonly IUIFactory _uiFactory;
        private readonly IWindowManager _windowManager;

        private LoadingScreenView _view;
        private UIPool<LoadingTaskText> _loadingTasksPoll;

        private LoadingTaskText _lastLoadingTask;

        public LoadingScreenWindow(IUIFactory uiFactory, IWindowManager windowManager)
        {
            _uiFactory = uiFactory;
            _windowManager = windowManager;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(OperationID.WARMUP, _warmup);
        }

        private UniTask<OperationResult> _warmup()
        {
            _uiFactory.Create(this, PrefabID.LoadingScreen, out _view);
            _view.gameObject.SetActive(false);

            _loadingTasksPoll = new UIPool<LoadingTaskText>(_uiFactory, PrefabID.LoadingScreenTask, _view.logContent);
            
            _windowManager.Register(this, WindowID.LoadingScreen, _view);
            
            return OperationResult.SuccessSync();
        }
        
        public void Open()
        {
            _view.progressButton.SetInteractable(false);
            _view.gameObject.SetActive(true);
        }

        public void Close()
        {
            _view.gameObject.SetActive(false);
        }

        private void _addTask(string task)
        {
            if (_loadingTasksPoll.GetEnabledCount() == MAX_TASKS_COUNT)
            {
                _loadingTasksPoll.Disable(_loadingTasksPoll.GetByIndex(0));
            }

            //TODO https://risenbeast.youtrack.cloud/issue/R-78/Loading-progress-in-ui-screen 
            _lastLoadingTask = _loadingTasksPoll.GetNext();
            _lastLoadingTask.SetTask(task);
        }

        private void _stopLoadLastTask(string duration)
        {
            //TODO https://risenbeast.youtrack.cloud/issue/R-78/Loading-progress-in-ui-screen 
            if (_lastLoadingTask != null)
            {
                _lastLoadingTask.Completed(duration);
                _lastLoadingTask = null;
            }
        }

        private void _setProgressValue()
        {
            //TODO https://risenbeast.youtrack.cloud/issue/R-78/Loading-progress-in-ui-screen 
            float normalizeValue = 0;
            
            _view.progressFill.SetValue(normalizeValue);
            _view.buttonText.SetText((normalizeValue * 100).ToString());
            
            if (normalizeValue >= 1)
            {
                _view.progressButton.SetInteractable(true);
            }
        }
    }
}