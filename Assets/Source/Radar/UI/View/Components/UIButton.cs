using System;
using UnityEngine;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class UIButton : UIView
    {
        public event Action Clicked;
        public UISelectable Selectable => _selectable;

        [SerializeField] private UISelectable _selectable;
        [SerializeField] private Button _button;
        
        public void Start()
        {
            _button.onClick.AddListener(_click);
        }

        public void SetInteractable(bool interactable)
        {
            _selectable.SetInteractable(interactable);
        }

        private void _click()
        {
            Clicked?.Invoke();
        }
    }
}