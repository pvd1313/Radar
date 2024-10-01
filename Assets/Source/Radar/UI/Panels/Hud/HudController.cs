using Beast.Claw.GameCycle;
using Beast.Radar.UI.Canvas;
using Beast.Radar.UI.Enums;
using Beast.Radar.UI.Panels.Base;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI.Panels.Hud
{
    public class HudController: BPanelController, Initializable, Disposable
    {
        private  HudProvider _hudProvider;
        private readonly ICanvas _canvasController;
        
        public HudController(ICanvas canvasController)
        {
            _canvasController = canvasController;
        }

        public UniTask<InitializationResult> Initialize()
        { 
            HudView view = _canvasController.CreateChildPanel<HudView>(isLastSabling: false);
            _hudProvider = new HudProvider(view);
            _hudProvider.Initialize();
            _canvasController.AddChildProvider(EPanelType.Hud, _hudProvider);
            return InitializationResult.SuccessNotAsync();
        }

        public UniTask<DispositionResult> Dispose()
        {
            _hudProvider.Dispose();
            return DispositionResult.SuccessNotAsync();
        }
    }
}