using TMPro;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class UIText : UIView
    {
        [SerializeField] private TextMeshProUGUI _text;

        public void SetText(string text)
        {
            _text.SetText(text);
        }
    }
}