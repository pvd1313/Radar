using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class BeastUISlider : BeastUIView, IPointerUpHandler
    {
        public Selectable Selectable => _selectable;

        public event Action<float> EndedChange;
        
        [SerializeField] private Slider _slider;
        [SerializeField] private Selectable _selectable;
        
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
        
        public void OnPointerUp(PointerEventData eventData)
        {
            EndedChange?.Invoke(_slider.value);
        }
    }
}