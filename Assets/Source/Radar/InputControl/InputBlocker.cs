using Beast.Claw.GameCycle;
using Beast.Radar.UI;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.InputControl
{
    public class InputBlocker: Initializable, IDisposable
    {
        private readonly IWindowManager _windowManager;
        private readonly IInputController _input;

        private int _blockCount;
        private bool _isBlock;

        public InputBlocker(IWindowManager windowManager, IInputController inputController)
        {
            _windowManager = windowManager;
            _input = inputController;
        }

        public UniTask<InitializationResult> Initialize()
        {
            Subscribe();
            return InitializationResult.SuccessNotAsync();
        }

        public void Dispose()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            _windowManager.MouseOverUI += OnMouseOverUI;
            _windowManager.WindowChanged += OnWindowChanged;
        }

        private void Unsubscribe()
        {
            _windowManager.MouseOverUI -= OnMouseOverUI;
            _windowManager.WindowChanged -= OnWindowChanged;
        }

        private void OnWindowChanged(bool isHasOpenedWindow)
        {
            if (isHasOpenedWindow)
            {
                _blockCount++;
            }
            else
            {
                _blockCount--;
            }

            RefreshBlockState();
        }

        private void OnMouseOverUI(bool over)
        {
            if (over)
            {
                _blockCount++;
            }
            else
            {
                _blockCount--;
            }
            
            RefreshBlockState();
        }

        private void RefreshBlockState()
        {
            if (_blockCount != 0 && !_isBlock)
            {
                _isBlock = true;
                _input.Block(_isBlock);
            }
            else if (_blockCount == 0 && _isBlock)
            {
                _isBlock = false;
                _input.Block(_isBlock);
            }
        }
    }
}