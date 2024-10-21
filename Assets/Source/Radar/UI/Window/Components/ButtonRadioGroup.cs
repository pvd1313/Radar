using System;

namespace Beast.Radar.UI
{
    public class ButtonRadioGroup<TButton> where TButton : UIView, IUIRadioButton<TButton> 
    {
        public event Action<TButton> Selected;
        public TButton SelectedButton { get; private set; }

        private TButton[] _buttons;

        public ButtonRadioGroup(TButton[] buttons)
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

        public void SetOtherGroup(TButton[] otherButtons)
        {
            _unsubscribeButtons();
            _buttons = otherButtons;
            _subscribeButtons();
        }

        private void _subscribeButtons()
        {
            foreach (TButton radioButton in _buttons)
            {
                radioButton.Clicked += _setSelected;
            }
        }   

        private void _unsubscribeButtons()
        {
            foreach (TButton radioButton in _buttons)
            {
                radioButton.Clicked -= _setSelected;
            }
        }

        private void _setSelected(TButton button)
        {
            if (SelectedButton != null)
            {
                if (SelectedButton.Equals(button))
                {
                    return;
                }
                SelectedButton.Deselect();
            }

            SelectedButton = button;

            Selected?.Invoke(SelectedButton);
        }
    }
}