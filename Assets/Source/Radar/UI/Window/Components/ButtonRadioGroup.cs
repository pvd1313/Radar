using System;

namespace Beast.Radar.UI
{
    public class UIButtonRadioGroup<T> where T : IUIRadioButton<T>
    {
        public event Action<T> Selected;
      
        private T[] _buttons;
        private T _selectedButton;

        public UIButtonRadioGroup(T[] buttons)
        {
            _buttons = buttons;
        }

        public void Warmup()
        {
            SubscribeButtons();
            _selectedButton = _buttons[0];
            _selectedButton.SetSelectedMode();
        }

        public void Dispose()
        {
            UnsubscribeButtons();
        }

        public void SetOtherGroup(T[] otherButtons)
        {
            UnsubscribeButtons();
            _buttons = otherButtons;
            SubscribeButtons();
            SetSelected(_buttons[0]);
        }

        private void SubscribeButtons()
        {
            foreach (T radioButton in _buttons)
            {
                radioButton.Clicked += SetSelected;
            }
        }

        private void UnsubscribeButtons()
        {
            foreach (T radioButton in _buttons)
            {
                radioButton.Clicked -= SetSelected;
            }
        }

        private void SetSelected(T button)
        {
            if (_selectedButton != null)
            {
                _selectedButton.SetDefaultMode();
            }

            _selectedButton = button;
            _selectedButton.SetSelectedMode();
            Selected?.Invoke(_selectedButton);
        }
    }
}