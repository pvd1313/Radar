using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class UISlider : UIView, IPointerUpHandler
    {
        public UISelectable Selectable => _selectable;
        public event Action<float> EndedChange;

        private const float ChangeSpeed = 1.5f; 
        
        [SerializeField] private Slider _slider;
        [SerializeField] private UISelectable _selectable;

        public void OnPointerUp(PointerEventData eventData)
        {
            EndedChange?.Invoke(_slider.value);
        }

        public void AddValue(float value)
        {
            _slider.value += value * ChangeSpeed;
        }
        
        public void SetValue(float value, bool withNotify)
        {
            if (withNotify)
            {
                _slider.value = value;
            }
            else
            {
                _slider.SetValueWithoutNotify(value);
            }
        }
    }
}