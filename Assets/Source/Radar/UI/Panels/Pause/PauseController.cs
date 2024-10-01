using Beast.Claw.GameCycle;
using Beast.Radar.UI.Canvas;
using Beast.Radar.UI.Enums;
using Beast.Radar.UI.Panels.Base;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI.Panels.Pause
{
    public class PauseController : BPanelController, Initializable, Disposable
    {
        private readonly ICanvas _canvas;
        private PauseProvider _pauseProvider;

        public PauseController(ICanvas canvas)
        {
            _canvas = canvas;
        }
        
        public UniTask<InitializationResult> Initialize()
        {
            PauseView view = _canvas.CreateChildPanel<PauseView>(isLastSabling: true);
            _pauseProvider = new PauseProvider(view);
            _pauseProvider.Initialize();
            _canvas.AddChildProvider(EPanelType.Pause, _pauseProvider);
            return InitializationResult.SuccessNotAsync();
        }

        public UniTask<DispositionResult> Dispose()
        {
            _pauseProvider.Dispose();
            return DispositionResult.SuccessNotAsync();
        }
    }
}