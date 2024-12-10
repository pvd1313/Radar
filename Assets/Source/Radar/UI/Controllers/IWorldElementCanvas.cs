using Noesis;
using Unity.Mathematics;

namespace Beast.Radar.UI
{
    public interface IWorldElementCanvas
    {
        void UpdatePosition(FrameworkElement element, float2 worldPosition); 
        
        void AddElement(UIElement element);
     
        void UpdateZIndex(UIElement element, int zIndex);
    }
}