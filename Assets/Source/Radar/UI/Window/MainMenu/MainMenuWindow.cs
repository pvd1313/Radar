using Beast.Claw.Attributes;
using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Claw.UI;
using Cysharp.Threading.Tasks;
using Noesis;

namespace Beast.Radar.UI
{
    public class MainMenuWindow : Actor, IWindow<Empty>
    {
        [Required] private readonly IWindowManager _windowManager;
        [Required] private readonly UIConfig _config;
        
        private MainMenuView _view;

        public UniTask Open(Empty data)
        {
            return UniTask.CompletedTask;
        }

        public UniTask Close()
        {
            return UniTask.CompletedTask;
        }

        protected override UniTask _warmup(ConcurrentContext context)
        {
            _windowManager.Register(this, WindowID.MAIN_MENU);
        
            _config.mainMenuAsset.Instantiate(out _view);
            
            _windowManager.Attach(_view, WindowOrder.MAIN_MENU);
            
            _view.Visibility = Visibility.Visible;
            
            return base._warmup(context);
        }
    }
}