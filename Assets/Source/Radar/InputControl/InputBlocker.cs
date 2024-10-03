using Beast.Claw.GameCycle;
using Beast.Radar.UI;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.InputControl
{
    public class InputBlocker: Initializable, Disposable
    {
        private readonly IWindowManager _windowManager;
        private readonly ICameraInputController _cameraInput;

        private int _blockCount;
        private bool _isBlock;

        public InputBlocker(IWindowManager windowManager, ICameraInputController cameraInputController)
        {
            _windowManager = windowManager;
            _cameraInput = cameraInputController;
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
                _cameraInput.Block(_isBlock);
            }
            else if (_blockCount == 0 && _isBlock)
            {
                _isBlock = false;
                _cameraInput.Block(_isBlock);
            }
        }
    }
}