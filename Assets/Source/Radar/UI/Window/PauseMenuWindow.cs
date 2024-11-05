using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Radar.InputHandling;
using Beast.Radar.Player;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class PauseMenuWindow : Controller, IWindow<Empty>, IGameCycleListener
    {
        private readonly CorePrefabLibrary _library;
        private readonly IWindowManager _windowManager;
        private readonly ISaveManager _saveManager;
        private readonly ISelectableZoneBuilder _zoneBuilder;
        private readonly IInputSystem _inputSystem;
        private readonly PlayerPreferences _playerPreferences;
        
        private PauseMenuView _view;
        private SelectablesController _selectablesController;
        private IWindowHandle<Empty> _mainHUD;
        private IWindowHandle<Empty> _mapGenerationWindow;
        
        public PauseMenuWindow(
            CorePrefabLibrary library,
            IWindowManager windowManager,
            ISaveManager saveManager,
            ISelectableZoneBuilder zoneBuilder,
            IInputSystem inputSystem,
            PlayerPreferences playerPreferences)
        {
            _library = library;
            _windowManager = windowManager;
            _saveManager = saveManager;
            _zoneBuilder = zoneBuilder;
            _inputSystem = inputSystem;
            _playerPreferences = playerPreferences;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
        }

        public UniTask Open(Empty data)
        {
            _selectablesController.Enable();
            _view.gameObject.SetActive(true);
            _view.loadButton.SetInteractable(false);

            bool saveExists = _saveManager.Exists(FolderName.DEFAULT_SAVE, FileName.DEFAULT_SAVE);
            _view.loadButton.SetInteractable(saveExists);

            _view.fpsToggle.SetValueWithoutNotify(_playerPreferences.isMaxFps);
            _view.pathfindingToggle.SetValueWithoutNotify(_playerPreferences.isAutoPathfinding);
            _refreshMapMovementText();
            
            return UniTask.CompletedTask;
        }

        public UniTask Close()
        {
            _view.gameObject.SetActive(false);

            _selectablesController.Disable();

            return UniTask.CompletedTask;
        }

        private UniTask _warmup(ConcurrentContext context)
        {
            _view = BScene.InstantiateMono(this, _library.pauseMenu, float3.zero, quaternion.identity);
            _view.gameObject.SetActive(false);
            _windowManager.Register(this, WindowID.Pause);
            _view.transform.SetParent(_windowManager.CanvasTransform, false);

            _mainHUD = _windowManager.GetHandle<Empty>(this, WindowID.MainHUD);
            _mapGenerationWindow = _windowManager.GetHandle<Empty>(this, WindowID.MapGeneration);

            _configureSelectables();
            _subscribeView();

            _selectablesController.Warmup();

            return UniTask.CompletedTask;
        }
        
        private void _configureSelectables()
        {
            Tuple<ESelectionZone, SelectableGridData> dpadGrid = _zoneBuilder.Build(
                zone: ESelectionZone.Navigate,
                selectables: new[]
                {
                    _view.newGameButton.Selectable,
                    _view.saveButton.Selectable,
                    _view.loadButton.Selectable,
                },
                isFillHorizontally: false,
                isLooping: true);

            _selectablesController = new SelectablesController(_inputSystem,
                new Dictionary<ESelectionZone, SelectableGridData>
                {
                    { dpadGrid.Item1, dpadGrid.Item2 }
                });
        }

        private void _subscribeView()
        {
            _view.closeButton.Clicked += () => { _mainHUD.Open(default); };

            _view.loadButton.Clicked += () =>
            {
                // TODO: https://risenbeast.youtrack.cloud/issue/R-64/Game-Loading-from-UI-block-or-new-window
                _saveManager.LoadAsync(FolderName.DEFAULT_SAVE, FileName.DEFAULT_SAVE);
                _mainHUD.Open(default);
            };

            _view.saveButton.Clicked += () =>
            {
                // TODO: https://risenbeast.youtrack.cloud/issue/R-64/Game-Loading-from-UI-block-or-new-window
                // Also should block UI during save
                _saveManager.Save(FolderName.DEFAULT_SAVE, FileName.DEFAULT_SAVE);
                _view.loadButton.SetInteractable(true);
            };

            _view.newGameButton.Clicked += () =>
            {
                _mapGenerationWindow.Open(default);
            };

            _view.fpsToggle.Changed += on =>
            {
                _playerPreferences.isMaxFps = on;
                _setFPS();
            };

            _view.pathfindingToggle.Changed += on =>
            {
                _playerPreferences.isAutoPathfinding = on;
            };
            
            _view.mapMovementButtons.Increment.Clicked += () =>
            {
                _playerPreferences.isSingleTapMovement = !_playerPreferences.isSingleTapMovement;
                _refreshMapMovementText();
            };
            
            _view.mapMovementButtons.Decrement.Clicked += () =>
            {
                _playerPreferences.isSingleTapMovement = !_playerPreferences.isSingleTapMovement;
                _refreshMapMovementText();
            };
        }

        private void _refreshMapMovementText()
        {
            string mapMovementMode = _playerPreferences.isSingleTapMovement ? "single tap" : "double tap";
            _view.mapMovementText.SetText(mapMovementMode);
            BDebug.LogInfo(this, $"set map movement value = {mapMovementMode}({_playerPreferences.isSingleTapMovement})");
        }

        private void _setFPS()
        {
            if (_playerPreferences.isMaxFps)
            {
                RefreshRate refreshRate = Screen.currentResolution.refreshRateRatio;
                float screenRate = (float)refreshRate.numerator / refreshRate.denominator;
                Application.targetFrameRate = (int)screenRate;
            }
            else
            {
                Application.targetFrameRate = 30;
            }
        }
    }
}