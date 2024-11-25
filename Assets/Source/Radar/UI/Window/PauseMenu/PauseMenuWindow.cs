using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Radar.Player;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using Noesis;
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
        
        private PauseWindowView _view;
        private IWindowHandle<Empty> _mainHUD;
        private IWindowHandle<Empty> _mapGenerationWindow;
        
        public PauseMenuWindow(
            CorePrefabLibrary library,
            IWindowManager windowManager,
            ISaveManager saveManager,
            PlayerPreference playerPreference,
            UIConfig uiConfig,
            INoesisWindowManager noesisWindowManager)
        {
            _library = library;
            _windowManager = windowManager;
            _saveManager = saveManager;
            _uiConfig = uiConfig;
            _noesisWindowManager = noesisWindowManager;
            _playerPreference = playerPreference;
        }

        public UniTask Open(Empty data)
        {
            _view.Visibility = Visibility.Visible;

            _view.toggleFPS.IsChecked = _playerPreferences.isMaxFps;

            /*
            _view.loadButton.SetInteractable(false);

            bool saveExists = _saveManager.Exists(FolderName.DEFAULT_SAVE, FileName.DEFAULT_SAVE);
            _view.loadButton.SetInteractable(saveExists);

            _view.fpsToggle.SetValueWithoutNotify(_playerPreferences.isMaxFps);
            _view.pathfindingToggle.SetValueWithoutNotify(_playerPreferences.isAutoPathfinding);
            _refreshMapMovementText();
            */
            return UniTask.CompletedTask;
        }

        public UniTask Close()
        {
           _view.Visibility = Visibility.Hidden;

           return UniTask.CompletedTask;
        }

        protected override UniTask _warmup(ConcurrentContext context)
        {
            _uiConfig.PauseWindowAsset.Instantiate(out _view);

            _noesisWindowManager.Attach(_view);
            
            _view.Visibility = Visibility.Hidden;
            
            _windowManager.Register(this, WindowID.PAUSE);
            
            _mainHUD = _windowManager.GetHandle<Empty>(this, WindowID.HUD);
            _mapGenerationWindow = _windowManager.GetHandle<Empty>(this, WindowID.MAP_GENERATION);

            _subscribeView();

            _setFPS();
            
            return UniTask.CompletedTask;
        }

        private void _subscribeView()
        {
            _view.buttonClose.Click += (sender, args) =>
            {
                BDebug.LogInfo(this, "press button close");
                _mainHUD.Open(default);
            };

            _view.toggleFPS.Click += (sender, args) =>
            {
                _playerPreferences.isMaxFps = _view.toggleFPS.IsChecked ?? false;
                _setFPS();
            };

            /*

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
            };*/
        }

        private void _refreshMapMovementText()
        {
            string mapMovementMode = _playerPreferences.beginMovementOnSingleTap ? "single tap" : "double tap";
            //_view.mapMovementText.SetText(mapMovementMode);
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