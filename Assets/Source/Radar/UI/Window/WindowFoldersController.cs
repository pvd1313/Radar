using Beast.Claw.Common;

namespace Beast.Radar.UI
{
    public class WindowFoldersController
    {
        public IWindow CurrentFolder { get; private set; }

        private readonly UIButton[] _buttons;
        private readonly IFolder[] _folders;
        private int _currentIndex;
        public WindowFoldersController(UIButton[] buttons, IFolder[] folders)
        {
            _buttons = buttons;
            _folders = folders;
            _buttons[_currentIndex].SetInteractable(false);
            CurrentFolder = _folders[_currentIndex]; 
            _folders[_currentIndex].Open();
        }
        
        public void Warmup()
        {
            for (int i = 0; i < _buttons.Length; i++)
            {
                int index = i;
                _buttons[i].Clicked += () => SwitchFolder(index);
            }    
            foreach (var folder in _folders)
            {
                folder.Warmup();
            }
        }

        public void Dispose()
        {
            foreach (var folder in _folders)
            {
                folder.Dispose();
            }
        }
        
        public void OpenNext()
        {
            int nextIndex = _currentIndex + 1;
            if (nextIndex >= _folders.Length)
            {
                nextIndex = 0;
            }
            SwitchFolder(nextIndex);
        }

        public void OpenPrevious()
        {
            int nextIndex = _currentIndex - 1;
            if (nextIndex < 0)
            {
                nextIndex = _folders.Length - 1;
            }
            SwitchFolder(nextIndex);
        }

        private void SwitchFolder(int index)
        {
            _buttons[_currentIndex].SetInteractable(true);
            _folders[_currentIndex].Close();
            _currentIndex = index;
            _buttons[_currentIndex].SetInteractable(false);
            _folders[_currentIndex].Open();
        }
    }
}