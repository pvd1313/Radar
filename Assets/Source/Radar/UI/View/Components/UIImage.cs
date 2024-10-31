using UnityEngine;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class UIImage : UIView
    {
        [SerializeField] private Image _image;

        public void SetAlpha(float alpha)
        {
            Color color = _image.color;
            color.a = alpha;
            _image.color = color;
        }

        public void SetSprite(Sprite sprite)
        {
            _image.sprite = sprite;
        }
    }
}