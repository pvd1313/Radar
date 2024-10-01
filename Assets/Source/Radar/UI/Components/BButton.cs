using System;
using Beast.Claw.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Beast.Radar.UI.Components
{
    public class BButton : BeastMono
    {
        public event Action Pressed;
        
        [SerializeField] private Button _button;
        
        private void Start()
        {
            _button.onClick.AddListener(Click);
        }
        
        private void Click()
        {
            Pressed?.Invoke();
        }

        public void SetInteractable(bool interactable)
        {
            _button.interactable = interactable;
        }
    }
}