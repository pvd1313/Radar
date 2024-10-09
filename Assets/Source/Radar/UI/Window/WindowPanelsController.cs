namespace Beast.Radar.UI
{
    public class WindowPanelsController
    {
        private readonly UIButton[] _buttons;
        private readonly IPanel[] _folders;
        private int _currentIndex;
        
        public WindowPanelsController(UIButton[] buttons, IPanel[] folders)
        {
            _buttons = buttons;
            _folders = folders;
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

        public void Enable()
        {
            _buttons[_currentIndex].SetInteractable(false);
            _folders[_currentIndex].Open();
        }

        public void Disable()
        {
            _folders[_currentIndex].Close();
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