using UnityEngine;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class UIFillingSlider : UIView
    {
        [SerializeField] private Image _image;

        public void SetValue(float value)
        {
            _image.fillAmount = value;
        }
    }
}