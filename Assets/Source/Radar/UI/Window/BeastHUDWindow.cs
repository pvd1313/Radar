using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public class BeastHUDWindow : IBeastUIWindow, IBeastGameCycleListener
    {
        private readonly IBeastUIFactory _uiFactory;
        private readonly IBeastWindowManager _windowManager;
        
        private BeastHUDView _view;

        public BeastHUDWindow(IBeastUIFactory uiFactory, IBeastWindowManager windowManager)
        {
            _uiFactory = uiFactory;
            _windowManager = windowManager;
        }
        
        public void ListenGameCycle(IBeastGameCycleHandle handle)
        {
            handle.ListenOperation(this, BeastCycleEventID.Warmup, Warmup);
            handle.ListenOperation(this, BeastCycleEventID.Dispose, Dispose);
        }
        
        private UniTask<BeastInitializationResult> Warmup()
        {
            _uiFactory.Create(this, BeastUIPrefabID.HudWindow, out _view);
                
            _windowManager.Register(this, BeastUIWindowID.MainHUD, _view);
                
            SubscribeView();
            
            return BeastInitializationResult.SuccessSync();
        }

        public UniTask<BeastInitializationResult> Dispose()
        {
            UnsubscribeView();

            return BeastInitializationResult.SuccessSync();
        }

        public void Open()
        {
           SetActive(true);
        }

        public void Close()
        {
            SetActive(false);
        }

        private void SetActive(bool isActive)
        {
            _view.pauseButton.SetInteractable(isActive);
        }
        
        private void SubscribeView()
        {
            _view.pauseButton.Clicked += OnClickedPauseButton;
        }

        private void UnsubscribeView()
        {
            _view.pauseButton.Clicked -= OnClickedPauseButton;
        }
        
        private void OnClickedPauseButton()
        {
            _windowManager.Show(BeastUIWindowID.Pause);
        }
    }
}