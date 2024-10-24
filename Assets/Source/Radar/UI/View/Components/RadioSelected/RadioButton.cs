using System;
using UnityEngine;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class RadioButton : UIView, IUIRadioButton<RadioButton>
    {
        public event Action<RadioButton> Clicked;
        public UISelectable Selectable => _selectable;
        public bool IsSelected { get; private set; }

        [SerializeField] private Button _button;
        [SerializeField] private UISelectable _selectable;
        [SerializeField] private SelectableVisual _selectableVisual;
        
        private void Start()
        {
            _button.onClick.AddListener(Click);
        }

        public void Select()
        {
            IsSelected = true;
            _selectableVisual.SetSelectedMode();
        }

        public void Click()
        {
            Clicked?.Invoke(this);
        }
        
        public void Deselect()
        {
            IsSelected = false;
            _selectableVisual.SetDefaultMode();
        }
    }
}