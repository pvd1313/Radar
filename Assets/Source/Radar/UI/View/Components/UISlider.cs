using System;
using UnityEngine;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class UISlider : UIView
    {
        public event Action<float> EndedChange;
        public UISelectable Selectable => _selectable;

        private const float ChangeSpeed = 1.5f;

        [SerializeField] private Slider _slider;
        [SerializeField] private UISelectable _selectable;

        private void Start()
        {
            _slider.onValueChanged.AddListener(value => EndedChange?.Invoke(value));
        }

        public void AddValue(float value)
        {
            _slider.value += value * ChangeSpeed;
        }

        public void SetValueWithNotify(float value)
        {
            _slider.value = value;
        }

        public void SetValueWithoutNotify(float value)
        {
            _slider.SetValueWithoutNotify(value);
        }
    }
}