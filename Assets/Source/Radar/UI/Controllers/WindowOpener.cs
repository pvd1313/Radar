using Beast.Claw.Common;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public class WindowOpener : Controller
    {
        private readonly IWindowHandle<LoadingWindowInput> _loadingScreen;
        private readonly IWindowHandle<Empty> _hud;
        private readonly IWindowHandle<Empty> _pause;
        private readonly IWindowHandle<Empty> _mapGeneration;
        private readonly IWindowHandle<MainMenuWindowInput> _mainMenu;

        public WindowOpener(IWindowManager manager)
        {
            _loadingScreen = manager.GetHandle<LoadingWindowInput>(this, WindowID.LOADING);
            _hud = manager.GetHandle<Empty>(this, WindowID.HUD);
            _pause = manager.GetHandle<Empty>(this, WindowID.PAUSE);
            _mapGeneration = manager.GetHandle<Empty>(this, WindowID.MAP_GENERATION);
            _mainMenu = manager.GetHandle<MainMenuWindowInput>(this, WindowID.MAIN_MENU);
        }

        public UniTask LoadingScreen(LoadingWindowInput input)
        {
            return _loadingScreen.Open(input);
        }

        public UniTask Pause()
        {
            return _pause.Open(default);
        }

        public UniTask MapGeneration()
        {
            return _mapGeneration.Open(default);
        }

        public UniTask HUD()
        {
            return _hud.Open(default);
        }
        
        public UniTask MainMenu(MainMenuWindowInput data)
        {
            return _mainMenu.Open(data);
        }
    }
}