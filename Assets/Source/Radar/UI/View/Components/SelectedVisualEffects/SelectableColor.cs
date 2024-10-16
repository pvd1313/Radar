using Beast.Claw.Debug;
using UnityEngine;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class SelectableColor : SelectableVisual
    {
        [SerializeField] private Image _image;
        [SerializeField] private Color _selectShade;

        private const float Blend = 0.5f;

        private Color _defaultColor;
        private bool _isSelect;


        public override void SetDefaultMode()
        {
            _image.color = _defaultColor;
            _isSelect = false;
        }

        public override void SetSelectedMode()
        {
            _defaultColor = _image.color;
            _image.color = Color.Lerp(_defaultColor, _selectShade, Blend);
            _isSelect = true;
        }

        public void SetDefaultColor(Color color)
        {
            _defaultColor = color;
            _image.color = _isSelect ? Color.Lerp(_defaultColor, _selectShade, Blend) : _defaultColor;
        }
    }
}