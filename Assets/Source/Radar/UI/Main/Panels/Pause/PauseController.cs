using Beast.Claw.GameCycle;
using Beast.Radar.GameSave;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public class PauseController : Initializable, Disposable
    {
        private readonly IPanelDispatcher _panelDispatcher;
        private readonly ISaveService _saveService;
        private PauseView _view;

        public PauseController(IPanelDispatcher panelDispatcher, ISaveService saveService)
        {
            _panelDispatcher = panelDispatcher;
            _saveService = saveService;
        }
        
        public UniTask<InitializationResult> Initialize()
        {
            _view = _panelDispatcher.AddPanel<PauseView>(UIComponentsIDs.PanelPause, isLastSabling: true);
            _view.ClosePanel();
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
                _view.loadButton.Clicked += OnClickedLoadButton;
                _view.saveButton.Clicked += OnClickedSaveButton; 
                _view.newGameButton.Clicked += OnClickedNewGameButton;
                foreach (var closeButton in _view.closePanelButtons)
                {
                    closeButton.Clicked += OnClickedClosePanelButton;
                }
            }
            else
            {
                _view.loadButton.Clicked -= OnClickedLoadButton;
                _view.saveButton.Clicked -= OnClickedSaveButton; 
                _view.newGameButton.Clicked -= OnClickedNewGameButton;
                foreach (var closeButton in _view.closePanelButtons)
                {
                    closeButton.Clicked -= OnClickedClosePanelButton;
                }
            }
        }

        private void OnClickedClosePanelButton()
        {
            _view.InvokeOtherPanelCall(UIComponentsIDs.PanelMainHUD);
        }

        private void OnClickedNewGameButton()
        {
            //todo invoke newGame action 
        }

        private void OnClickedSaveButton()
        {
            _saveService.SaveData();
            //todo invoke save action 
        }

        private void OnClickedLoadButton()
        {
            _saveService.LoadData();
            //todo invoke load action 
        }
    }
}