using Beast.Claw.GameCycle;
using Beast.Radar.UI;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.InputControl
{
    public class InputBlockerOnUIHover: Initializable, Disposable
    {
        private readonly IPanelDispatcher _panelDispatcher;
        private readonly IInputController _input;

        public InputBlockerOnUIHover(IPanelDispatcher panelDispatcher, IInputController inputController)
        {
            _panelDispatcher = panelDispatcher;
            _input = inputController;
        }

        public UniTask<InitializationResult> Initialize()
        {
            _panelDispatcher.MouseOverUI += OnMouseOverUI;
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