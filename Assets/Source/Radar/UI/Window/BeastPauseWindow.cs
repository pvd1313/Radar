using Beast.Claw.GameCycle;
using Beast.Radar.InputControl;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public class BeastPauseWindow : IBeastUIWindow, IBeastInitialize, IBeastDispose
    {
        private readonly IBeastUIFactory _uiFactory;
        private readonly IBeastWindowManager _windowManager;
        private readonly IBeastUIInputController _uiInput;

        private BeastUISelectionLoop _selectionLoop;
        private BeastPauseMenuView _menuView;

        public BeastPauseWindow(
            IBeastUIFactory uiFactory,
            IBeastWindowManager windowManager,
            IBeastUIInputController uiInputController)
        {
            _uiFactory = uiFactory;
            _windowManager = windowManager;
            _uiInput = uiInputController;
        }

        public UniTask<BeastInitializationResult> Initialize()
        {
            _uiFactory.Create(this, BeastUIPrefabID.PauseWindow, out _menuView);
            _windowManager.Register(this, BeastUIWindowID.Pause, _menuView);

            _selectionLoop = new BeastUISelectionLoop(_uiInput, new[]
            {
                _menuView.newGameButton.Selectable,
                _menuView.saveButton.Selectable,
                _menuView.loadButton.Selectable,
            });

            Subscribe();
            return BeastInitializationResult.SuccessNotAsync();
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
            foreach (BeastUIButton closeButton in _menuView.closePanelButtons)
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
            foreach (BeastUIButton closeButton in _menuView.closePanelButtons)
            {
                closeButton.Clicked += OnClickedClosePanelButton;
            }
        }

        private void Unsubscribe()
        {
            _menuView.loadButton.Clicked -= OnClickedLoadButton;
            _menuView.saveButton.Clicked -= OnClickedSaveButton;
            _menuView.newGameButton.Clicked -= OnClickedNewGameButton;
            foreach (BeastUIButton closeButton in _menuView.closePanelButtons)
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
            _windowManager.Show(BeastUIWindowID.MapGeneration);
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