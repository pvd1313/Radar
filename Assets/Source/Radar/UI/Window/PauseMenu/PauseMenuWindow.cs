using Beast.Claw.Attributes;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Radar.Player;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using Noesis;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class PauseMenuWindow : Actor, IWindow<Empty>
    {
        [Required] private readonly IWindowManager _windowManager;
        [Required] private readonly ISaveManager _saveManager;
        [Required] private readonly PlayerPreference _playerPreference;
        [Required] private readonly UIConfig _uiConfig;
        [Required] private readonly IGraphyWindow _graphyWindow;

        private PauseWindowView _view;
        
        public UniTask Open(Empty data)
        {
            _view.Visibility = Visibility.Visible;

            _view.toggleFPS.IsChecked = _playerPreference.isMaxFps;
            
            _view.toggleGraphy.IsChecked = _playerPreference.graphy.isShowGraphy;

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

            _windowManager.Attach(_view, 1);
            
            _view.Visibility = Visibility.Collapsed;
            
            _windowManager.Register(this, WindowID.PAUSE);
            
            _subscribeView();

            _setFPS();
            
            return UniTask.CompletedTask;
        }

        private void _subscribeView()
        {
            _view.buttonClose.Click += (_, _) =>
            {
                _windowManager.Open.HUD();
            };

            _view.toggleFPS.Click += (_, _) =>
            {
                _playerPreference.isMaxFps = _view.toggleFPS.IsChecked ?? false;
                _setFPS();
            };

            _view.toggleGraphy.Click += (_, _) =>
            {
                _playerPreference.graphy.isShowGraphy = _view.toggleGraphy.IsChecked ?? false;
                if (_playerPreference.graphy.isShowGraphy)
                {
                    _graphyWindow.Open();
                }
                else
                {
                    _graphyWindow.Close();
                }
            };
            
            _view.buttonLoadGame.Click += async (_, _) =>
            {
                // TODO: https://risenbeast.youtrack.cloud/issue/R-64/Game-Loading-from-UI-block-or-new-window
                await _saveManager.LoadAsync(FolderName.DEFAULT_SAVE, FileName.DEFAULT_SAVE);

                await _windowManager.Open.HUD();
            };

            _view.buttonSaveGame.Click += (_, _) =>
            {
                // TODO: https://risenbeast.youtrack.cloud/issue/R-64/Game-Loading-from-UI-block-or-new-window
                // Also should block UI during save
                _saveManager.Save(FolderName.DEFAULT_SAVE, FileName.DEFAULT_SAVE);
                _view.buttonLoadGame.IsEnabled = true;
            };

            _view.buttonNewGame.Click += (_, _) =>
            {
                _windowManager.Open.MapGeneration();
            };

            
            _view.buttonNextMovement.Click += (_, _) =>
            {
                _playerPreference.beginMovementOnSingleTap = !_playerPreference.beginMovementOnSingleTap;
                _refreshMapMovementText();
            };
            
            _view.buttonPreviousMovement.Click += (_, _) =>
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