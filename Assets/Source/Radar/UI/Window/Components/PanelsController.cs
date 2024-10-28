namespace Beast.Radar.UI
{
    public class PanelsController
    {
        private readonly UIButton[] _buttons;
        private readonly IPanel[] _folders;
        private int _currentIndex;
        
        public PanelsController(UIButton[] buttons, IPanel[] folders)
        {
            _buttons = buttons;
            _folders = folders;
        }
        
        public void Warmup()
        {
            for (int i = 0; i < _buttons.Length; i++)
            {
                int index = i;
                _buttons[i].Clicked += () => _switchFolder(index);
            }    
            foreach (IPanel folder in _folders)
            {
                folder.Warmup();
            }
        }

        public void Dispose()
        {
            foreach (IPanel folder in _folders)
            {
                folder.Dispose();
            }
        }

        public void Enable()
        {
            _buttons[_currentIndex].SetInteractable(false);
            _folders[_currentIndex].Open(default);
        }

        public void Disable()
        {
            _folders[_currentIndex].Close();
        }
        
        private void _switchFolder(int index)
        {
            _buttons[_currentIndex].SetInteractable(true);
            _folders[_currentIndex].Close();
            _currentIndex = index;
            _buttons[_currentIndex].SetInteractable(false);
            _folders[_currentIndex].Open(default);
        }
    }
}