using Beast.Claw.UI;
using Beast.Claw.Validation;
using Noesis;

namespace Beast.Radar.UI
{
    public class PlayerView : BeastControl
    {
        public Rectangle player;
        private ResourceDictionary _resources;
        
        public override void BindElements()
        {
            _resources = Resources;
            
            _find("view_player", out player);
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
    }
}