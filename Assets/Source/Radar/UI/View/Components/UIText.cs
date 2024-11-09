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

        public void SetTime(int hours, int minutes)
        {
            _text.SetText("{0:00}:{1:00}", hours, minutes);
        }
    }
}