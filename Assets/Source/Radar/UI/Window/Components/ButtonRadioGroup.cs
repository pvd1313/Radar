using System;

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
            SubscribeButtons();
            SelectedButton = _buttons[0];
            SelectedButton.SetSelectedMode();
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
            if (SelectedButton != null)
            {
                SelectedButton.SetDefaultMode();
            }

            SelectedButton = button;
            SelectedButton.SetSelectedMode();
            Selected?.Invoke(SelectedButton);
        }
    }
}