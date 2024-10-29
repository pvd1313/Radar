using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Radar.InputHandling;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public class PauseMenuWindow : Controller, IWindow<Empty>, IGameCycleListener
    {
        private readonly IUIFactory _uiFactory;
        private readonly IWindowManager _windowManager;
        private readonly ISaveManager _saveManager;
        private readonly ISelectableZoneBuilder _zoneBuilder;
        private readonly IInputSystem _inputSystem;

        private PauseMenuView _menuView;
        private SelectablesController _selectablesController;
        private IWindowHandle<Empty> _mainHUD;
        private IWindowHandle<Empty> _mapGenerationWindow;

        public PauseMenuWindow(
            IUIFactory uiFactory,
            IWindowManager windowManager,
            ISaveManager saveManager,
            ISelectableZoneBuilder zoneBuilder,
            IInputSystem inputSystem)
        {
            _uiFactory = uiFactory;
            _windowManager = windowManager;
            _saveManager = saveManager;
            _zoneBuilder = zoneBuilder;
            _inputSystem = inputSystem;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(ActionID.WARMUP, _warmup);
            handle.ListenOperation(ActionID.DISPOSE, _dispose);
        }

        public async UniTask Open(Empty data)
        {
            _selectablesController.Enable();
            _menuView.gameObject.SetActive(true);
            _menuView.loadButton.SetInteractable(false);

            bool saveExists = await _saveManager.Exists(SaveManager.DEFAULT_FOLDER, SaveManager.DEFAULT_FILE);
            _menuView.loadButton.SetInteractable(saveExists);
        }

        public UniTask Close()
        {
            _hideView();
            _selectablesController.Disable();

            return UniTask.CompletedTask;
        }

        private UniTask<ActionResult> _warmup(ActionContext context)
        {
            _uiFactory.Create(this, PrefabID.PauseWindow, out _menuView);
            _hideView();
            _windowManager.Register(this, WindowID.Pause);
            _menuView.transform.SetParent(_windowManager.CanvasTransform, false);

            _mainHUD = _windowManager.GetHandle<Empty>(this, WindowID.MainHUD);
            _mapGenerationWindow = _windowManager.GetHandle<Empty>(this, WindowID.MapGeneration);
            
            _configureSelectables();
            _subscribeView();
            
            _selectablesController.Warmup();
            
            return ActionResult.SuccessSyncronous();
        }

        private UniTask<ActionResult> _dispose(ActionContext context)
        {
            _unsubscribeView();
            
            _selectablesController.Dispose();
            
            return ActionResult.SuccessSyncronous();
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

            _selectablesController = new SelectablesController(_inputSystem, new Dictionary<ESelectionZone, SelectableGridData>
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
            _mainHUD.Open(default);
        }

        private void _onClickedNewGameButton()
        {
            _mapGenerationWindow.Open(default);
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
            _mainHUD.Open(default);
        }

        private void _hideView()
        {
            _menuView.gameObject.SetActive(false);
        }
    }
}