using System;
using Beast.Claw.Debug;

namespace Beast.Radar.UI
{
    public class ButtonRadioGroup<T> where T : IUIRadioButton<T>
    {
        public T SelectedButton { get; private set; }
        public event Action<T> Selected;
      
        private T[] _buttons;

        public ButtonRadioGroup(T[] buttons)
        {
            _buttons = buttons;
        }
        
        public void Warmup()
        {
            _subscribeButtons();
            SelectedButton = _buttons[0];
            SelectedButton.Select();
        }

        public void Dispose()
        {
            _unsubscribeButtons();
        }

        public void SetOtherGroup(T[] otherButtons)
        {
            _unsubscribeButtons();
            _buttons = otherButtons;
            _subscribeButtons();
        }

        private void _subscribeButtons()
        {
            foreach (T radioButton in _buttons)
            {
                radioButton.Clicked += _setSelected;
            }
        }   

        private void _unsubscribeButtons()
        {
            foreach (T radioButton in _buttons)
            {
                radioButton.Clicked -= _setSelected;
            }
        }

        private void _setSelected(T button)
        {
            if (SelectedButton != null)
            {
                SelectedButton.Deselect();
            }

            SelectedButton = button;
          
            Selected?.Invoke(SelectedButton);
        }
    }
}