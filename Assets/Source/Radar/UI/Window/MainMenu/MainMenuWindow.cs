using Beast.Claw.Attributes;
using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using Noesis;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class MainMenuWindow : Actor, IWindow<Empty>
    {
        [Required] private readonly IWindowManager _windowManager;
        [Required] private readonly UIConfig _config;
        [Required] private readonly ISaveManager _saveManager;

        private MainMenuView _view;
        
        public UniTask Open(Empty data)
        {
            _view.buttonContinue.IsEnabled = _saveManager.Exists(FolderName.DEFAULT_SAVE, FileName.DEFAULT_SAVE);
            
            _view.Visibility = Visibility.Visible;
            
            return UniTask.CompletedTask;
        }

        public UniTask Close()
        {
            _view.Visibility = Visibility.Collapsed;
            
            return UniTask.CompletedTask;
        }

        protected override UniTask _warmup(ConcurrentContext context)
        {
            _windowManager.Register(this, WindowID.MAIN_MENU);

            _config.mainMenuAsset.Instantiate(out _view);

            _windowManager.Attach(_view, WindowOrder.MAIN_MENU);

            _view.Visibility = Visibility.Collapsed;
            
            _view.AddVersionDescription("v.1", "- description\n- description");

            _subscribe();

            return UniTask.CompletedTask;
        }

        private void _subscribe()
        {
            _view.buttonYoutube.Click += (_, _) =>
            {
                Application.OpenURL("https://www.youtube.com/@risenbeast-xxx");
            };
            
            _view.buttonDiscord.Click += (_, _) =>
            {
                Application.OpenURL("https://discord.gg/wV97uSE5Cs");
            };

            _view.buttonContinue.Click += async (_, _) =>
            {
                ProgressTracker tracker = new();

                tracker.Begin(1, 1);
                
                await _windowManager.Open.LoadingScreen(new LoadingWindowInput
                {
                    clearLastProgress = true,
                    currentProgress = 0,
                    nextAction = () => _windowManager.Open.HUD(),
                    progressTracker = tracker
                });
                
                await tracker.Next("Loading game save");
                
                await _saveManager.LoadAsync(FolderName.DEFAULT_SAVE, FileName.DEFAULT_SAVE);
                
                await tracker.Next("Waiting for player input");
            };
            
            _view.buttonNewGame.Click += (_, _) =>
            {
                _windowManager.Open.MapGeneration(new MapGenerationWindowData
                {
                    cancelAction = () => _windowManager.Open.MainMenu()
                });
            };

            // TODO: https://risenbeast.youtrack.cloud/issue/R-115/Options-and-about-UI-windows
            _view.buttonOptions.IsEnabled = false;
            
            // TODO: https://risenbeast.youtrack.cloud/issue/R-115/Options-and-about-UI-windows
            _view.buttonAbout.IsEnabled = false;
        }
    }
}