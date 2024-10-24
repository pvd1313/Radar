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
        
        public void Warmup()
        {
            _subscribeButtons();
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
            _setSelected(_buttons[0]);
        }

        public void Select(int index)
        {
            _setSelected(_buttons[index]);
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
            SelectedButton.Select();

            Selected?.Invoke(SelectedButton);
        }
    }
}