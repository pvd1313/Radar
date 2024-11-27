using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Radar.Player;
using Beast.Radar.Serialization;
using Beast.Radar.UI.Graphy;
using Cysharp.Threading.Tasks;
using Noesis;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class PauseMenuWindow : Actor, IWindow<Empty>
    {
        private readonly IWindowManager _windowManager;
        private readonly ISaveManager _saveManager;
        private readonly PlayerPreference _playerPreference;
        private readonly UIConfig _uiConfig;
        private readonly INoesisWindowManager _noesisWindowManager;
        private readonly IGraphyWindow _graphyWindow;

        private PauseWindowView _view;
        private IWindowHandle<Empty> _mainHUD;
        private IWindowHandle<Empty> _mapGenerationWindow;
        
        public PauseMenuWindow(
            IWindowManager windowManager,
            ISaveManager saveManager,
            PlayerPreference playerPreference,
            UIConfig uiConfig,
            INoesisWindowManager noesisWindowManager,
            IGraphyWindow graphyWindow)
        {
            _windowManager = windowManager;
            _saveManager = saveManager;
            _uiConfig = uiConfig;
            _noesisWindowManager = noesisWindowManager;
            _graphyWindow = graphyWindow;
            _playerPreference = playerPreference;
        }

        public UniTask Open(Empty data)
        {
            _view.Visibility = Visibility.Visible;

            _view.toggleFPS.IsChecked = _playerPreference.isMaxFps;
            
            _view.toggleGraphy.IsChecked = _playerPreference.isShowGraphy;

            _view.buttonLoadGame.IsEnabled = _saveManager.Exists(FolderName.DEFAULT_SAVE, FileName.DEFAULT_SAVE);
            
            _refreshMapMovementText();
           
            return UniTask.CompletedTask;
        }

        public UniTask Close()
        {
           _view.Visibility = Visibility.Collapsed;

           return UniTask.CompletedTask;
        }

        protected override UniTask _warmup(ConcurrentContext context)
        {
            _uiConfig.pauseWindowAsset.Instantiate(out _view);

            _noesisWindowManager.Attach(_view);
            
            _view.Visibility = Visibility.Collapsed;
            
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
                _playerPreference.isMaxFps = _view.toggleFPS.IsChecked ?? false;
                _setFPS();
            };

            _view.toggleGraphy.Click += (sender, args) =>
            {
                _playerPreference.isShowGraphy = _view.toggleGraphy.IsChecked ?? false;
                if (_playerPreference.isShowGraphy)
                {
                    _graphyWindow.Open();
                }
                else
                {
                    _graphyWindow.Close();
                }
            };
            
            _view.buttonLoadGame.Click += (sender, args) =>
            {
                // TODO: https://risenbeast.youtrack.cloud/issue/R-64/Game-Loading-from-UI-block-or-new-window
                _saveManager.LoadAsync(FolderName.DEFAULT_SAVE, FileName.DEFAULT_SAVE);
                _mainHUD.Open(default);
            };

            _view.buttonSaveGame.Click += (sender, args) =>
            {
                // TODO: https://risenbeast.youtrack.cloud/issue/R-64/Game-Loading-from-UI-block-or-new-window
                // Also should block UI during save
                _saveManager.Save(FolderName.DEFAULT_SAVE, FileName.DEFAULT_SAVE);
                _view.buttonLoadGame.IsEnabled = true;
            };

            _view.buttonNewGame.Click += (sender, args) =>
            {
                _mapGenerationWindow.Open(default);
            };

            
            _view.buttonNextMovement.Click += (sender, args) =>
            {
                _playerPreference.beginMovementOnSingleTap = !_playerPreference.beginMovementOnSingleTap;
                _refreshMapMovementText();
            };
            
            _view.buttonPreviousMovement.Click += (sender, args) =>
            {
                _playerPreference.beginMovementOnSingleTap = !_playerPreference.beginMovementOnSingleTap;
                _refreshMapMovementText();
            };
        }

        private void _refreshMapMovementText()
        {
            string mapMovementMode = _playerPreference.beginMovementOnSingleTap ? "single tap" : "double tap";
            _view.textMovement.Text = mapMovementMode;
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