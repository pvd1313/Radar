using System;
using Beast.Claw.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class UIButtonView : BeastMono
    {
        public event Action Clicked;
        
        [SerializeField] private Button _button;
        
        private void Start()
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