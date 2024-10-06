using System;
using UnityEngine;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class UIButton : UIView
    {
        public Selectable Selectable => _selectable;
        public event Action Clicked;

        [SerializeField] private Selectable _selectable;
        [SerializeField] private Button _button;
        
        public void Start()
        {
            _button.onClick.AddListener(Click);
        }
        
        private void Click()
        {
            Clicked?.Invoke();
        }

        public void SetInteractable(bool interactable)
        {
            _button.interactable = interactable;
        }
    }
}