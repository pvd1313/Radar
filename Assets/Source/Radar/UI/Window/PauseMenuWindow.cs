using Beast.Claw.GameCycle;
using Beast.Radar.GameCycle;
using Beast.Radar.InputControl;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public class PauseMenuWindow : IWindow, IGameCycleListener
    {
        private readonly IUIFactory _uiFactory;
        private readonly IWindowManager _windowManager;
        private readonly IUIInputController _uiInput;
        private readonly IGameSaveManager _gameSave;

        private UISelectionLoop _selectionLoop;
        private PauseMenuView _menuView;

        public PauseMenuWindow(
            IUIFactory uiFactory,
            IWindowManager windowManager,
            IUIInputController uiInputController,
            IGameSaveManager gameSave)
        {
            _uiFactory = uiFactory;
            _windowManager = windowManager;
            _uiInput = uiInputController;
            _gameSave = gameSave;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Warmup, Warmup);
            handle.ListenOperation(this, OperationID.Dispose, Dispose);
        }
        
        private UniTask<OperationResult> Warmup()
        {
            _uiFactory.Create(this, PrefabID.PauseWindow, out _menuView);
                
            _windowManager.Register(this, WindowID.Pause, _menuView);

            _selectionLoop = new UISelectionLoop(_uiInput, new[]
            {
                _menuView.newGameButton.Selectable,
                _menuView.saveButton.Selectable,
                _menuView.loadButton.Selectable,
            });

            SubscribeView();
            
            return OperationResult.SuccessSync();
        }

        private UniTask<OperationResult> Dispose()
        {
            UnsubscribeView();
            
            return OperationResult.SuccessSync();
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
            
            foreach (UIButton closeButton in _menuView.closePanelButtons)
            {
                closeButton.SetInteractable(isActive);
            }

            _menuView.gameObject.SetActive(isActive);
        }

        private void SubscribeView()
        {
            _menuView.loadButton.Clicked += OnClickedLoadButton;
            _menuView.saveButton.Clicked += OnClickedSaveButton;
            _menuView.newGameButton.Clicked += OnClickedNewGameButton;
            
            foreach (UIButton closeButton in _menuView.closePanelButtons)
            {
                closeButton.Clicked += OnClickedClosePanelButton;
            }
        }

        private void UnsubscribeView()
        {
            _menuView.loadButton.Clicked -= OnClickedLoadButton;
            _menuView.saveButton.Clicked -= OnClickedSaveButton;
            _menuView.newGameButton.Clicked -= OnClickedNewGameButton;
            
            foreach (UIButton closeButton in _menuView.closePanelButtons)
            {
                closeButton.Clicked -= OnClickedClosePanelButton;
            }
        }

        private void OnClickedClosePanelButton()
        {
            _windowManager.Show(WindowID.MainHUD);
        }

        private void OnClickedNewGameButton()
        {
            _windowManager.Show(WindowID.MapGeneration);
        }

        private void OnClickedSaveButton()
        {
            _gameSave.Save("default", "default"); 
        }

        private void OnClickedLoadButton()
        {
            _gameSave.Load("default", "default");
        }
    }
}