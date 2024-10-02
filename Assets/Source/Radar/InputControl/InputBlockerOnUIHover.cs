using Beast.Claw.GameCycle;
using Beast.Radar.UI;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.InputControl
{
    public class InputBlockerOnUIHover: Initializable, IDisposable
    {
        private readonly IWindowManager _windowManager;
        private readonly IInputController _input;

        public InputBlockerOnUIHover(IWindowManager windowManager, IInputController inputController)
        {
            _windowManager = windowManager;
            _input = inputController;
        }

        public UniTask<InitializationResult> Initialize()
        {
            _windowManager.MouseOverUI += OnMouseOverUI;
            return InitializationResult.SuccessNotAsync();
        }

        public void Dispose()
        {
            _windowManager.MouseOverUI -= OnMouseOverUI;
        }

        private void OnMouseOverUI(bool over)
        {
            _input.Block(over);
        }
    }
}