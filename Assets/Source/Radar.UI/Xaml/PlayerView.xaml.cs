using Beast.Claw.UI;
using Beast.Claw.Validation;
using Noesis;
using Unity.Mathematics;

namespace Beast.Radar.UI
{
    public class PlayerView : BeastControl
    {
        public Rectangle view;
        private ResourceDictionary _resources;
        private float2 _pivot;

        public override void BindElements()
        {
            _resources = Resources;
            
            _find("view_player", out view);
            
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

        public float2 GetOffset()
        {
            float size = view.ActualWidth / 2;
            return new float2(size) - _pivot * size;
        }
    }
}