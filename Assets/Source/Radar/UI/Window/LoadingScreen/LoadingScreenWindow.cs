using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

namespace Beast.Radar.UI
{
    public class LoadingScreenWindow : IWindow<Empty>, IGameCycleListener
    {
        private const int MAX_TASK_COUNT = 20;

        private readonly UIPrefabLibrary _library;
        private readonly IWindowManager _windowManager;

        private LoadingScreenView _view;
        private UIPool<LoadingTaskText> _loadingTasksPool;

        private LoadingTaskText _lastLoadingTask;

        public LoadingScreenWindow(UIPrefabLibrary library, IWindowManager windowManager)
        {
            _library = library;
            _windowManager = windowManager;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(ActionID.WARMUP, _warmup);
        }

        private UniTask<ActionResult> _warmup(ActionContext context)
        {
            _view = BScene.InstantiateMono(this, _library.loadingScreen, float3.zero, quaternion.identity);
            _view.gameObject.SetActive(false);

            _loadingTasksPool = new UIPool<LoadingTaskText>(_library.loadingTaskText, _view.logContent);
            
            _windowManager.Register(this, WindowID.LoadingScreen);
            _view.transform.SetParent(_windowManager.CanvasTransform, false);
            
            return ActionResult.SuccessSyncronous();
        }
        
        public UniTask Open(Empty data)
        {
            _view.progressButton.SetInteractable(false);
            _view.gameObject.SetActive(true);
            
            return UniTask.CompletedTask;
        }

        public UniTask Close()
        {
            _view.gameObject.SetActive(false);
            
            return UniTask.CompletedTask;
        }

        private void _addTask(string task)
        {
            if (_loadingTasksPool.GetEnabledCount() == MAX_TASK_COUNT)
            {
                _loadingTasksPool.Disable(_loadingTasksPool.GetByIndex(0));
            }

            //TODO https://risenbeast.youtrack.cloud/issue/R-78/Loading-progress-in-ui-screen 
            _lastLoadingTask = _loadingTasksPool.GetNext();
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