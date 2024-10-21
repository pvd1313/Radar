using System;
using System.Collections.Generic;
using Beast.Claw.GameCycle;
using Beast.Radar.InputControl;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public class PauseMenuWindow : IWindow, IGameCycleListener
    {
        private readonly IUIFactory _uiFactory;
        private readonly IWindowManager _windowManager;
        private readonly ISaveManager _saveManager;
        private readonly ISelectableZoneBuilder _zoneBuilder;
        private readonly IUIInputController _input;

        private PauseMenuView _menuView;
        private SelectablesController _selectablesController;

        public PauseMenuWindow(
            IUIFactory uiFactory,
            IWindowManager windowManager,
            ISaveManager saveManager,
            ISelectableZoneBuilder zoneBuilder,
            IUIInputController input)
        {
            _uiFactory = uiFactory;
            _windowManager = windowManager;
            _saveManager = saveManager;
            _zoneBuilder = zoneBuilder;
            _input = input;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(OperationID.WARMUP, _warmup);
            handle.ListenOperation(OperationID.DISPOSE, _dispose);
        }

        public async void Open()
        {
            _selectablesController.Enable();
            _menuView.gameObject.SetActive(true);
            _menuView.loadButton.SetInteractable(false);

            bool saveExists = await _saveManager.Exists(SaveManager.DEFAULT_FOLDER, SaveManager.DEFAULT_FILE);
            _menuView.loadButton.SetInteractable(saveExists);
        }

        public void Close()
        {
            _menuView.gameObject.SetActive(false);
            _selectablesController.Disable();
        }

        private UniTask<OperationResult> _warmup()
        {
            _uiFactory.Create(this, PrefabID.PauseWindow, out _menuView);
            _windowManager.Register(this, WindowID.Pause, _menuView);
            
            _configureSelectables();
            _subscribeView();
            
            _selectablesController.Warmup();
            
            return OperationResult.SuccessSync();
        }

        private UniTask<OperationResult> _dispose()
        {
            _unsubscribeView();
            
            _selectablesController.Dispose();
            
            return OperationResult.SuccessSync();
        }

        private void _configureSelectables()
        {
            Tuple<ESelectionZone, SelectableGridData> dpadGrid = _zoneBuilder.Build(
                zone: ESelectionZone.Navigate, 
                selectables: new[]
                {
                    _menuView.newGameButton.Selectable,
                    _menuView.saveButton.Selectable,
                    _menuView.loadButton.Selectable,
                }, 
                isFillHorizontally: false, 
                isLooping: true);

            _selectablesController = new SelectablesController(_input, new Dictionary<ESelectionZone, SelectableGridData>
            {
                { dpadGrid.Item1, dpadGrid.Item2 }
            });
        }

        private void _subscribeView()
        {
            _menuView.loadButton.Clicked += _onClickedLoadButton;
            _menuView.saveButton.Clicked += _onClickedSaveButton;
            _menuView.newGameButton.Clicked += _onClickedNewGameButton;

            foreach (UIButton closeButton in _menuView.closePanelButtons)
            {
                closeButton.Clicked += _onClickedClosePanelButton;
            }
        }

        private void _unsubscribeView()
        {
            _menuView.loadButton.Clicked -= _onClickedLoadButton;
            _menuView.saveButton.Clicked -= _onClickedSaveButton;
            _menuView.newGameButton.Clicked -= _onClickedNewGameButton;

            foreach (UIButton closeButton in _menuView.closePanelButtons)
            {
                closeButton.Clicked -= _onClickedClosePanelButton;
            }
        }

        private void _onClickedClosePanelButton()
        {
            _windowManager.Show(WindowID.MainHUD);
        }

        private void _onClickedNewGameButton()
        {
            _windowManager.Show(WindowID.MapGeneration);
        }

        private async void _onClickedSaveButton()
        {
            // TODO: https://risenbeast.youtrack.cloud/issue/R-64/Game-Loading-from-UI-block-or-new-window
            // Also should block UI during save
            await _saveManager.Save(SaveManager.DEFAULT_FOLDER, SaveManager.DEFAULT_FILE);
            
            _menuView.loadButton.SetInteractable(true);
        }

        private void _onClickedLoadButton()
        {
            // TODO: https://risenbeast.youtrack.cloud/issue/R-64/Game-Loading-from-UI-block-or-new-window
            _saveManager.Load(SaveManager.DEFAULT_FOLDER, SaveManager.DEFAULT_FILE);
        }
    }
}