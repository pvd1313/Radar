using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public class PauseWindow : Initializable, Disposable
    {
        private readonly IUIFactory _uiFactory;
        private readonly IWindowManager _windowManager;

        private PauseView _view;

        public PauseWindow(IUIFactory uiFactory, IWindowManager windowManager)
        {
            _uiFactory = uiFactory;
            _windowManager = windowManager;
        }

        public UniTask<InitializationResult> Initialize()
        {
            _view = _uiFactory.CreateWindowView(UIPrefabsID.PanelPause) as PauseView;
            _windowManager.RegisterPanel(UIWindowID.Pause, _view);
            Subscribe();
            return InitializationResult.SuccessNotAsync();
        }

        public void Dispose()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            _view.loadButton.Clicked += OnClickedLoadButton;
            _view.saveButton.Clicked += OnClickedSaveButton;
            _view.newGameButton.Clicked += OnClickedNewGameButton;
            foreach (var closeButton in _view.closePanelButtons)
            {
                closeButton.Clicked += OnClickedClosePanelButton;
            }
        }

        private void Unsubscribe()
        {
            _view.loadButton.Clicked -= OnClickedLoadButton;
            _view.saveButton.Clicked -= OnClickedSaveButton;
            _view.newGameButton.Clicked -= OnClickedNewGameButton;
            foreach (var closeButton in _view.closePanelButtons)
            {
                closeButton.Clicked -= OnClickedClosePanelButton;
            }
        }

        private void OnClickedClosePanelButton()
        {
            _windowManager.ShowWindow(UIWindowID.MainHUD);
        }

        private void OnClickedNewGameButton()
        {
            //todo invoke newGame action 
        }

        private void OnClickedSaveButton()
        {
            //todo invoke save action 
        }

        private void OnClickedLoadButton()
        {
            //todo invoke load action 
        }
    }
}