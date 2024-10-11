using Beast.Claw.GameCycle;
using Beast.Radar.GameCycle;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public class HUDWindow : IWindow, IGameCycleListener
    {
        private readonly IUIFactory _uiFactory;
        private readonly IWindowManager _windowManager;

        private HUDView _view;
        
        public HUDWindow(IUIFactory uiFactory, IWindowManager windowManager)
        {
            _uiFactory = uiFactory;
            _windowManager = windowManager;
        }

        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Warmup, Warmup);
            handle.ListenOperation(this, OperationID.Dispose, Dispose);
        }

        public void Open()
        {
           SetActive(true);
        }

        public void Close()
        {
            SetActive(false);
        }

        private UniTask<OperationResult> Warmup()
        {
            _uiFactory.Create(this, PrefabID.HudWindow, out _view);
                
            _windowManager.Register(this, WindowID.MainHUD, _view);
                
            SubscribeView();
            
            return OperationResult.SuccessSync();
        }

        private UniTask<OperationResult> Dispose()
        {
            UnsubscribeView();

            return OperationResult.SuccessSync();
        }

        private void SetActive(bool isActive)
        {
            _view.pauseButton.SetInteractable(isActive);
        }
        
        private void SubscribeView()
        {
            _view.pauseButton.Clicked += OnClickedPauseButton;
        }

        private void UnsubscribeView()
        {
            _view.pauseButton.Clicked -= OnClickedPauseButton;
        }
        
        private void OnClickedPauseButton()
        {
            _windowManager.Show(WindowID.Pause);
        }
    }
}