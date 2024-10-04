using Beast.Claw.GameCycle;
using Beast.Radar.GameSave;
using Beast.Radar.InputControl;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public class PauseWindow : BeastUIWindow, Initializable, Disposable
    {
        private readonly IUIFactory _uiFactory;
        private readonly IWindowManager _windowManager;
        private readonly IUIInputController _uiInput;
        private readonly ISaveService _saveService;

        private UISelectionLoop _selectionLoop;
        private PauseView _view;

        public PauseWindow(
            IUIFactory uiFactory, 
            IWindowManager windowManager, 
            IUIInputController uiInputController,
            ISaveService saveService)
        {
            _uiFactory = uiFactory;
            _windowManager = windowManager;
            _uiInput = uiInputController;
            _saveService = saveService;
        }

        public UniTask<InitializationResult> Initialize()
        {
            _view = _uiFactory.CreateWindowView(UIPrefabID.PauseWindow) as PauseView;
            _windowManager.RegisterWindow(UIWindowID.Pause, _view, this);

            _selectionLoop = new UISelectionLoop(_uiInput, new[]
            {
                _view.newGameButton.Selectable,
                _view.saveButton.Selectable,
                _view.loadButton.Selectable,
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
            _view.loadButton.SetInteractable(isActive);
            _view.saveButton.SetInteractable(isActive);
            _view.newGameButton.SetInteractable(isActive);
            foreach (var closeButton in _view.closePanelButtons)
            {
                closeButton.SetInteractable(isActive);
            }

            _view.gameObject.SetActive(isActive);
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
            _saveService.SaveData();
        }

        private void OnClickedLoadButton()
        {
            _saveService.LoadData();
        }
    }
}