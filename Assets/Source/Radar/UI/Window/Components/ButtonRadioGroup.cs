using System;
using Beast.Claw.Debug;

namespace Beast.Radar.UI
{
    public class ButtonRadioGroup<T> where T :  UIView, IUIRadioButton<T> 
    {
        public T SelectedButton { get; private set; }
        public event Action<T> Selected;
      
        private T[] _buttons;

        public ButtonRadioGroup(T[] buttons)
        {
            _buttons = buttons;
        }
        
        public void Warmup(int startIndex = 0)
        {
            _subscribeButtons();
            SelectedButton = _buttons[startIndex];
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
            BDebug.LogInfo(this, $"select {button.name}");
            if (SelectedButton != null)
            {
                if (SelectedButton.Equals(button))
                {
                    return;
                }
                BDebug.LogInfo(this, $"deselect {SelectedButton.name}");
                SelectedButton.Deselect();
            }

            SelectedButton = button;

            Selected?.Invoke(SelectedButton);
        }
    }
}