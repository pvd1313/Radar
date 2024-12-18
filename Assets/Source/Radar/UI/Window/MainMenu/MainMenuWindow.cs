using Beast.Claw.Attributes;
using Beast.Claw.GameLoop;
using Cysharp.Threading.Tasks;
using Noesis;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class MainMenuWindow : Actor, IWindow<MainMenuWindowInput>
    {
        [Required] private readonly IWindowManager _windowManager;
        [Required] private readonly UIConfig _config;

        private MainMenuView _view;
        
        public UniTask Open(MainMenuWindowInput data)
        {
            _view.button_continue.IsEnabled = data.isSaveExist;
            
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

            return base._warmup(context);
        }

        private void _subscribe()
        {
            _view.button_youtube.Click += (_, _) =>
            {
                Application.OpenURL("https://www.youtube.com/@risenbeast-xxx");
            };
            
            _view.button_discord.Click += (_, _) =>
            {
                Application.OpenURL("https://discord.gg/wV97uSE5Cs");
            };

            _view.button_continue.Click += (_, _) =>
            {
                _windowManager.Open.HUD();
            };
            
            _view.button_newGame.Click += (_, _) =>
            {
                _windowManager.Open.MapGeneration();
            };

            //todo: https://risenbeast.youtrack.cloud/issue/R-115/Options-and-about-UI-windows
            _view.button_options.IsEnabled = false;
            
            //todo: https://risenbeast.youtrack.cloud/issue/R-115/Options-and-about-UI-windows
            _view.button_about.IsEnabled = false;
        }
    }
}