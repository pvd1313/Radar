using Noesis;
using Unity.Mathematics;

namespace Beast.Radar.UI
{
    public interface IWorldElementCanvas
    {
        void UpdatePosition(UIElement element, float2 worldPosition, float2 offset); 
        
        void AddElement(UIElement element);
     
        void UpdateZIndex(UIElement element, int zIndex);
    }
}