using Beast.Claw.Common;
using Beast.Radar.UI.Canvas;
using Beast.Radar.UI.Panels.Base;
using Unity.Mathematics;

namespace Beast.Radar.UI.Factory
{
    public class UIFactory : IUIFactory
    {
        private readonly UILibrary _library;

        public UIFactory(UILibrary library)
        {
            _library = library;
        }

        public CanvasView CreateCanvas()
        {
            return CreatePanel<CanvasView>();
        }
        
        public T CreatePanel<T>() where T : BView
        {
            if (_library.TryGetViewPrefab(out T viewPrefab))
            {
                return BeastScene.InstantiateMono(this,viewPrefab, float3.zero, quaternion.identity);
            }

            throw new BeastException($"UI library cannot find a view prefab of type {typeof(T)}");
        }
    }
}