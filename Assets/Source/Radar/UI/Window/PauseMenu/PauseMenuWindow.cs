using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Radar.Player;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class PauseMenuWindow : Actor, IWindow<Empty>
    {
        private readonly CorePrefabLibrary _library;
        private readonly IWindowManager _windowManager;
        private readonly ISaveManager _saveManager;
        private readonly PlayerPreference _playerPreference;
        
        private PauseMenuView _view;
        private IWindowHandle<Empty> _mainHUD;
        private IWindowHandle<Empty> _mapGenerationWindow;
        
        public PauseMenuWindow(
            CorePrefabLibrary library,
            IWindowManager windowManager,
            ISaveManager saveManager,
            PlayerPreference playerPreference)
        {
            _library = library;
            _windowManager = windowManager;
            _saveManager = saveManager;
            _playerPreference = playerPreference;
        }

        public UniTask Open(Empty data)
        {
            _view.gameObject.SetActive(true);
            _view.loadButton.SetInteractable(false);

            bool saveExists = _saveManager.Exists(FolderName.DEFAULT_SAVE, FileName.DEFAULT_SAVE);
            _view.loadButton.SetInteractable(saveExists);

            _view.fpsToggle.SetValueWithoutNotify(_playerPreference.isMaxFps);
            _view.pathfindingToggle.SetValueWithoutNotify(_playerPreference.isAutoPathfinding);
            _refreshMapMovementText();
            
            return UniTask.CompletedTask;
        }

        public UniTask Close()
        {
            _view.gameObject.SetActive(false);

            return UniTask.CompletedTask;
        }

        protected override UniTask _warmup(ConcurrentContext context)
        {
            _view = BScene.InstantiateMono(this, _library.pauseMenu);
            _view.gameObject.SetActive(false);
            _windowManager.Register(this, WindowID.PAUSE);
            _view.transform.SetParent(_windowManager.CanvasTransform, false);

            _mainHUD = _windowManager.GetHandle<Empty>(this, WindowID.HUD);
            _mapGenerationWindow = _windowManager.GetHandle<Empty>(this, WindowID.MAP_GENERATION);

            _subscribeView();

            _setFPS();
            
            return UniTask.CompletedTask;
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
                _playerPreference.isMaxFps = on;
                _setFPS();
            };

            _view.pathfindingToggle.Changed += on =>
            {
                _playerPreference.isAutoPathfinding = on;
            };
            
            _view.mapMovementButtons.Increment.Clicked += () =>
            {
                _playerPreference.beginMovementOnSingleTap = !_playerPreference.beginMovementOnSingleTap;
                _refreshMapMovementText();
            };
            
            _view.mapMovementButtons.Decrement.Clicked += () =>
            {
                _playerPreference.beginMovementOnSingleTap = !_playerPreference.beginMovementOnSingleTap;
                _refreshMapMovementText();
            };
        }

        private void _refreshMapMovementText()
        {
            string mapMovementMode = _playerPreference.beginMovementOnSingleTap ? "single tap" : "double tap";
            _view.mapMovementText.SetText(mapMovementMode);
        }

        private void _setFPS()
        {
            if (_playerPreference.isMaxFps)
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