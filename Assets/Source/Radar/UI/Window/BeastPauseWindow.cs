using Beast.Claw.GameCycle;
using Beast.Radar.InputControl;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public class BeastPauseWindow : IBeastUIWindow, Initializable, Disposable
    {
        private readonly IBeastUIFactory _uiFactory;
        private readonly IBeastWindowManager _windowManager;
        private readonly IUIInputController _uiInput;

        private BeastUISelectionLoop _selectionLoop;
        private BeastCorePauseMenuView _menuView;

        public BeastPauseWindow(IBeastUIFactory uiFactory, IBeastWindowManager windowManager, IUIInputController uiInputController)
        {
            _uiFactory = uiFactory;
            _windowManager = windowManager;
            _uiInput = uiInputController;
        }

        public UniTask<InitializationResult> Initialize()
        {
            _menuView = _uiFactory.NewWindow(BeastUIPrefabID.PauseWindow) as BeastCorePauseMenuView;
            _windowManager.Register(this, BeastUIWindowID.Pause, _menuView);

            _selectionLoop = new BeastUISelectionLoop(_uiInput, new[]
            {
                _menuView.newGameButton.Selectable,
                _menuView.saveButton.Selectable,
                _menuView.loadButton.Selectable,
            });

            Subscribe();
            return InitializationResult.SuccessNotAsync();
        }

        public void Dispose()
        {
            Unsubscribe();
        }

        public void Open()
        {
            _selectionLoop.Enable();
            SetActive(true);
        }

        public void Close()
        {
            SetActive(false);
            _selectionLoop.Disable();
        }

        private void SetActive(bool isActive)
        {
            _menuView.loadButton.SetInteractable(isActive);
            _menuView.saveButton.SetInteractable(isActive);
            _menuView.newGameButton.SetInteractable(isActive);
            foreach (var closeButton in _menuView.closePanelButtons)
            {
                closeButton.SetInteractable(isActive);
            }

            _menuView.gameObject.SetActive(isActive);
        }

        private void Subscribe()
        {
            _menuView.loadButton.Clicked += OnClickedLoadButton;
            _menuView.saveButton.Clicked += OnClickedSaveButton;
            _menuView.newGameButton.Clicked += OnClickedNewGameButton;
            foreach (var closeButton in _menuView.closePanelButtons)
            {
                closeButton.Clicked += OnClickedClosePanelButton;
            }
        }

        private void Unsubscribe()
        {
            _menuView.loadButton.Clicked -= OnClickedLoadButton;
            _menuView.saveButton.Clicked -= OnClickedSaveButton;
            _menuView.newGameButton.Clicked -= OnClickedNewGameButton;
            foreach (var closeButton in _menuView.closePanelButtons)
            {
                closeButton.Clicked -= OnClickedClosePanelButton;
            }
        }

        private void OnClickedClosePanelButton()
        {
            _windowManager.Show(BeastUIWindowID.MainHUD);
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