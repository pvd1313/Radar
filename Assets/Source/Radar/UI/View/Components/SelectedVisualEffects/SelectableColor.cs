using UnityEngine;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class SelectableColor : SelectableVisual
    {
        [SerializeField] private Image _image;
        [SerializeField] private Color _selectShade;
        [SerializeField, Range(0,1)] private  float _blend = 0.5f;

        private Color _defaultColor;
        private bool _isSelect;
        
        public override void SetDefaultMode()
        {
            _image.color = _defaultColor;
            _isSelect = false;
        }

        public override void SetSelectedMode()
        {
            if (_isSelect)
            {
                return;
            }
            _defaultColor = _image.color;
            _image.color = Color.Lerp(_defaultColor, _selectShade, _blend);
            _isSelect = true;
        }

        public void SetDefaultColor(Color color)
        {
            _defaultColor = color;
            _image.color = _isSelect ? Color.Lerp(_defaultColor, _selectShade, _blend) : _defaultColor;
        }
    }
}