using Noesis;

namespace Beast.Radar.UI
{
    public interface INoesisWindowManager
    {
        void Attach(UIElement uiElement);
        void Attach(UIElement uiElement, int index);
    }
}