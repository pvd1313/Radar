using Beast.Claw.GameCycle;
using Beast.Radar.UI.Canvas;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.InputControl
{
    public class InputBlockerOnUIHover: Initializable, Disposable
    {
        private readonly ICanvas _canvas;
        private readonly IInputController _input;

        public InputBlockerOnUIHover(ICanvas canvas, IInputController inputController)
        {
            _canvas = canvas;
            _input = inputController;
        }

        public UniTask<InitializationResult> Initialize()
        {
            _canvas.MouseOverUI += OnMouseOverUI;
            return InitializationResult.SuccessNotAsync();
        }

        private void OnMouseOverUI(bool over)
        {
            _input.Block(over);
        }

        public UniTask<DispositionResult> Dispose()
        {
            
            return DispositionResult.SuccessNotAsync();
        }
    }
}