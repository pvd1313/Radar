using System.Collections.Generic;
using Beast.Claw.GameCycle;
using Beast.Radar.GameCycle;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

namespace Beast.Radar.UI
{
    public class PauseMenuWindow : IWindow, IGameCycleListener
    {
        public Dictionary<ESelectionZone, SelectableGridData> Selections { get; private set; }
        
        private readonly IUIFactory _uiFactory;
        private readonly IWindowManager _windowManager;
        private readonly IGameSaveManager _gameSave;
        private readonly ISelectionZoneBuilder _selectionZoneBuilder;

        private PauseMenuView _menuView;
        
        public PauseMenuWindow(
            IUIFactory uiFactory,
            IWindowManager windowManager,
            IGameSaveManager gameSave,
            ISelectionZoneBuilder selectionZoneBuilder)
        {
            _uiFactory = uiFactory;
            _windowManager = windowManager;
            _gameSave = gameSave;
            _selectionZoneBuilder = selectionZoneBuilder;
        }

        public void ListenGameCycle(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Warmup, Warmup);
            handle.ListenOperation(this, OperationID.Dispose, Dispose);
        }

        private UniTask<OperationResult> Warmup()
        {
            _uiFactory.Create(this, PrefabID.PauseWindow, out _menuView);
                
            _windowManager.Register(this, WindowID.Pause, _menuView);
            
            SelectableGridData dpadSelectable = _selectionZoneBuilder.Build(new[]
            {
                _menuView.newGameButton.Selectable,
                _menuView.saveButton.Selectable,
                _menuView.loadButton.Selectable,
            }, new int2(1, 3), isLooping: true);

            Selections = new Dictionary<ESelectionZone, SelectableGridData>()
            {
                { ESelectionZone.DPad, dpadSelectable }
            };
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
            SetActive(true);
        }

        public void Close()
        {
            SetActive(false);
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