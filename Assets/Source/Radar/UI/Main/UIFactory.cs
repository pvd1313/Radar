using Beast.Claw.Common;
using Unity.Mathematics;

namespace Beast.Radar.UI
{
    public class UIFactory : IUIFactory
    {
        private readonly UILibrary _library;

        public UIFactory(UILibrary library)
        {
            _library = library;
        }

        public CanvasView CreateCanvas(string canvasPrefabID)
        {
            CanvasView canvasPrefab = _library.GetCanvas(canvasPrefabID);
            CanvasView view = BeastScene.InstantiateMono(this, canvasPrefab, float3.zero, quaternion.identity);
            return view;
        }

        public BeastUIWindowView CreateWindowView(string panelPrefabID)
        {
            BeastUIWindowView windowPrefab = _library.GetPanel(panelPrefabID);
            BeastUIWindowView view = BeastScene.InstantiateMono(this, windowPrefab, float3.zero, quaternion.identity);
            return view;
        }
    }
}