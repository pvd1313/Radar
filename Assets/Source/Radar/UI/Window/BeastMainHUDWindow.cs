using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public class BeastMainHUDWindow : IBeastUIWindow, Initializable, Disposable
    {
        private readonly IBeastUIFactory _uiFactory;
        private readonly IBeastWindowManager _windowManager;
        
        private BeastCoreHUDView _view;

        public BeastMainHUDWindow(IBeastUIFactory uiFactory, IBeastWindowManager windowManager)
        {
            _uiFactory = uiFactory;
            _windowManager = windowManager;
        }

        public UniTask<InitializationResult> Initialize()
        { 
            _view = _uiFactory.NewWindow(BeastUIPrefabID.MainHudWindow) as BeastCoreHUDView;
            _windowManager.Register(this, BeastUIWindowID.MainHUD, _view);
            Subscribe();
            Open();
            return InitializationResult.SuccessNotAsync();
        }

        public void Dispose()
        {
            Unsubscribe();
        }

        public void Open()
        {
           SetActive(true);
        }

        public void Close()
        {
            SetActive(false);
        }

        private void SetActive(bool isActive)
        {
            _view.pauseButton.SetInteractable(isActive);
        }
        
        private void Subscribe()
        {
            _view.pauseButton.Clicked += OnClickedPauseButton;
        }

        private void Unsubscribe()
        {
            _view.pauseButton.Clicked -= OnClickedPauseButton;
        }
        
        private void OnClickedPauseButton()
        {
            _windowManager.Show(BeastUIWindowID.Pause);
        }
    }
}