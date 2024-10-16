using System;
using Beast.Claw.Debug;
using UnityEngine;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class IntRadioButton : UIView, IUIRadioButton<IntRadioButton>
    {
        public UISelectable Selectable => _selectable;
        public bool IsSelected { get; private set; }
        public int Value => _value;
        public event Action<IntRadioButton> Clicked;

        [SerializeField] private int _value;
        [SerializeField] private Button _button;
        [SerializeField] private UISelectable _selectable;
        [SerializeField] private SelectableVisual _selectableVisual;
        
        private void Start()
        {
            _button.onClick.AddListener(Select);
        }

        public void Select()
        {
            IsSelected = true;
            _selectableVisual.SetSelectedMode();
            BDebug.LogInfo(this, $"{_value} select");
            Clicked?.Invoke(this);
        }

        public void Deselect()
        {
            IsSelected = false;
            _selectableVisual.SetDefaultMode();
            BDebug.LogInfo(this, $"{_value} deselect");
        }
    }
}