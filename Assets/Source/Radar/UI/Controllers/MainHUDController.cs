using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public class MainHUDController :  Initializable, Disposable
    {
        private readonly IPanelDispatcher _panelDispatcherController;
        
        private  MainHUDView _view;
        
        public MainHUDController(IPanelDispatcher panelDispatcherController)
        {
            _panelDispatcherController = panelDispatcherController;
        }

        public UniTask<InitializationResult> Initialize()
        { 
            _view = _panelDispatcherController.AddPanel<MainHUDView>(UIComponentsIDs.PanelMainHUD, isLastSabling: true);
            SubscribeToEvents(true);
            return InitializationResult.SuccessNotAsync();
        }

        public UniTask<DispositionResult> Dispose()
        {
            SubscribeToEvents(false);
            return DispositionResult.SuccessNotAsync();
        }

        private void SubscribeToEvents(bool flag)
        {
            if (flag)
            {
                _view.pauseButton.Clicked += OnClickedPauseButton;
            }
            else
            {
                _view.pauseButton.Clicked -= OnClickedPauseButton;
            }
        }

        private void OnClickedPauseButton()
        {
            _view.InvokeOtherPanelCall(UIComponentsIDs.PanelPause);
        }
    }
}