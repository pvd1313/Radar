using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public class MainHUDWindow : Initializable, Disposable
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
            _windowManager.RegisterPanel(UIWindowID.MainHUD, _view);
            Subscribe();
            return InitializationResult.SuccessNotAsync();
        }

        public void Dispose()
        {
            Unsubscribe();
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