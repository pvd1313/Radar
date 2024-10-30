using System;
using UnityEngine;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class UIToggle : UIView
    {
        public UISelectable Selectable => _selectable;
        public event Action<bool> Changed;

        [SerializeField] private Toggle _toggle;
        [SerializeField] private UISelectable _selectable;

        private void Awake()
        {
            _toggle.onValueChanged.AddListener(on => Changed?.Invoke(on));
        }

        public void SetValueWithNotify(bool value)
        {
            _toggle.isOn = value;
        }

        public void SetValueWithoutNotify(bool value)
        {
            _toggle.SetIsOnWithoutNotify(value);
        }
    }
}