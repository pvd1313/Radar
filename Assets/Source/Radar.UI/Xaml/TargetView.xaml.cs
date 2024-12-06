

using Beast.Claw.UI;
using Beast.Claw.Validation;
using Noesis;

namespace Beast.Radar.UI
{
    public class TargetView : BeastControl
    {
        public Canvas target;
        public TextBlock distanceText;
        
        private ResourceDictionary _resources;

        public override void BindElements()
        {
            _resources = Resources;
            _find("canvas_target", out target);
            _find("text_distance", out distanceText);
        }
        
        public void SetViewSize(float iconSize)
        {
            if (_resources.Contains("Size.Icon"))
            {
                _resources["Size.Icon"] = iconSize;
            }
            else
            {
                throw new ValidationException("Resource 'Size.Icon' not found.");
            }
        }

        public void SetFontSize(float fontSize)
        {
            if (_resources.Contains("Size.Font"))
            {
                _resources["Size.Font"] = fontSize;
            }
            else
            {
                throw new ValidationException("Resource 'Size.Icon' not found.");
            }
        }
    }
}
