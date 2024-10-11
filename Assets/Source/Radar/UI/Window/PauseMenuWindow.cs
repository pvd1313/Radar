using System;
using System.Collections.Generic;
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
        private readonly IGameSaveManager _gameSave;
        private readonly ISelectionZoneBuilder _zoneBuilder;
        private readonly IUIInputController _input;

        private PauseMenuView _menuView;
        private SelectableController _selectableController;

        public PauseMenuWindow(
            IUIFactory uiFactory,
            IWindowManager windowManager,
            IGameSaveManager gameSave,
            ISelectionZoneBuilder zoneBuilder,
            IUIInputController input)
        {
            _uiFactory = uiFactory;
            _windowManager = windowManager;
            _gameSave = gameSave;
            _zoneBuilder = zoneBuilder;
            _input = input;
        }

        public void ListenGameCycle(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Warmup, Warmup);
            handle.ListenOperation(this, OperationID.Dispose, Dispose);
        }

        public void Open()
        {
            _selectableController.Enable();
            _menuView.gameObject.SetActive(true);
        }

        public void Close()
        {
            _menuView.gameObject.SetActive(false);
            _selectableController.Disable();
        }

        private UniTask<OperationResult> Warmup()
        {
            _uiFactory.Create(this, PrefabID.PauseWindow, out _menuView);
            _windowManager.Register(this, WindowID.Pause, _menuView);
            
            ConfigureSelectables();
            SubscribeView();
            
            _selectableController.Warmup();
            
            return OperationResult.SuccessSync();
        }

        private UniTask<OperationResult> Dispose()
        {
            UnsubscribeView();
            
            _selectableController.Dispose();
            
            return OperationResult.SuccessSync();
        }

        private void ConfigureSelectables()
        {
            Tuple<ESelectionZone, SelectableGridData> dpadGrid = _zoneBuilder.Build(
                zone: ESelectionZone.DPad, 
                selectables: new[]
                {
                    _menuView.newGameButton.Selectable,
                    _menuView.saveButton.Selectable,
                    _menuView.loadButton.Selectable,
                }, 
                isFillHorizontally: false, 
                isLooping: true);

            _selectableController = new SelectableController(_input, new Dictionary<ESelectionZone, SelectableGridData>
            {
                { dpadGrid.Item1, dpadGrid.Item2 }
            });
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