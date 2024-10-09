using System;

namespace Beast.Radar.UI
{
    public class WindowFoldersController
    {
        public IFolder CurrentFolder { get; private set; }

        public event Action<IFolder, IFolder> FolderSwitched;
        
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
        
        private void SwitchFolder(int index)
        {
            _buttons[_currentIndex].SetInteractable(true);
            _folders[_currentIndex].Close();
            FolderSwitched?.Invoke(_folders[_currentIndex], _folders[index]);
            _currentIndex = index;
            _buttons[_currentIndex].SetInteractable(false);
            _folders[_currentIndex].Open();
        }
    }
}