using Beast.Claw.UI;
using Beast.Claw.Validation;
using Noesis;
using Unity.Mathematics;

namespace Beast.Radar.UI
{
    public class TargetView : BeastControl
    {
        public Canvas view;
        public TextBlock distanceText;

        private float2 _pivot;
        private ResourceDictionary _resources;

        public override void BindElements()
        {
            _resources = Resources;
            _find("canvas_target", out view);
            _find("text_distance", out distanceText);
            _pivot = new float2(view.RenderTransformOrigin.X, view.RenderTransformOrigin.Y);
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

        public float2 GetOffset()
        {
            float size = view.ActualWidth / 2;
            return new float2(size) - _pivot * size;
        }
    }
}