using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public class MainHUDWindow : BeastUIWindow, Initializable, Disposable
    {
        private readonly IUIFactory _uiFactory;
        private readonly IWindowManager _windowManager;
        
        private MainHUDView _view;

        public MainHUDWindow(IUIFactory uiFactory, IWindowManager windowManager)
        {
            _uiFactory = uiFactory;
            _windowManager = windowManager;
        }

        public UniTask<InitializationResult> Initialize()
        { 
            _view = _uiFactory.CreateWindowView(UIPrefabsID.WindowMainHUD) as MainHUDView;
            _windowManager.RegisterWindow(UIWindowID.MainHUD, _view, this);
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
            _windowManager.ShowWindow(UIWindowID.Pause);
        }
    }
}