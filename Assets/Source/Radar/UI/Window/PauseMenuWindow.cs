using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Radar.InputHandling;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

namespace Beast.Radar.UI
{
    public class PauseMenuWindow : Controller, IWindow<Empty>, IGameCycleListener
    {
        private readonly CorePrefabLibrary _library;
        private readonly IWindowManager _windowManager;
        private readonly ISaveManager _saveManager;
        private readonly ISelectableZoneBuilder _zoneBuilder;
        private readonly IInputSystem _inputSystem;

        private PauseMenuView _menuView;
        private SelectablesController _selectablesController;
        private IWindowHandle<Empty> _mainHUD;
        private IWindowHandle<Empty> _mapGenerationWindow;

        public PauseMenuWindow(
            CorePrefabLibrary library,
            IWindowManager windowManager,
            ISaveManager saveManager,
            ISelectableZoneBuilder zoneBuilder,
            IInputSystem inputSystem)
        {
            _library = library;
            _windowManager = windowManager;
            _saveManager = saveManager;
            _zoneBuilder = zoneBuilder;
            _inputSystem = inputSystem;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
            handle.ListenBlocking(ActionID.DISPOSE, _dispose);
        }

        public UniTask Open(Empty data)
        {
            _selectablesController.Enable();
            _menuView.gameObject.SetActive(true);
            _menuView.loadButton.SetInteractable(false);

            bool saveExists = _saveManager.Exists(SaveManager.DEFAULT_FOLDER, SaveManager.DEFAULT_FILE);
            _menuView.loadButton.SetInteractable(saveExists);
            
            return UniTask.CompletedTask;
        }

        public UniTask Close()
        {
            _hideView();
            _selectablesController.Disable();

            return UniTask.CompletedTask;
        }

        private UniTask _warmup(ConcurrentContext context)
        {
             _menuView = BScene.InstantiateMono(this, _library.pauseMenu, float3.zero, quaternion.identity);
            _hideView();
            _windowManager.Register(this, WindowID.Pause);
            _menuView.transform.SetParent(_windowManager.CanvasTransform, false);

            _mainHUD = _windowManager.GetHandle<Empty>(this, WindowID.MainHUD);
            _mapGenerationWindow = _windowManager.GetHandle<Empty>(this, WindowID.MapGeneration);
            
            _configureSelectables();
            _subscribeView();
            
            _selectablesController.Warmup();
            
            return UniTask.CompletedTask;
        }

        private void _dispose(BlockingContext context)
        {
            _unsubscribeView();
            
            _selectablesController.Dispose();
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

        private void _onClickedSaveButton()
        {
            // TODO: https://risenbeast.youtrack.cloud/issue/R-64/Game-Loading-from-UI-block-or-new-window
            // Also should block UI during save
            _saveManager.Save(SaveManager.DEFAULT_FOLDER, SaveManager.DEFAULT_FILE);
            
            _menuView.loadButton.SetInteractable(true);
        }

        private void _onClickedLoadButton()
        {
            // TODO: https://risenbeast.youtrack.cloud/issue/R-64/Game-Loading-from-UI-block-or-new-window
            _saveManager.LoadAsync(SaveManager.DEFAULT_FOLDER, SaveManager.DEFAULT_FILE);
        }

        private void _hideView()
        {
            _menuView.gameObject.SetActive(false);
        }
    }
}