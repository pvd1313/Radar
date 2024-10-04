using Beast.Claw.Common;
using Unity.Mathematics;

namespace Beast.Radar.UI
{
    public class BeastUIFactory : IBeastUIFactory
    {
        private readonly BeastPrefabLibrary _prefabs;

        public BeastUIFactory(BeastPrefabLibrary prefabs)
        {
            _prefabs = prefabs;
        }

        public BeastCanvasView NewCanvas(string canvasPrefabID)
        {
            BeastCanvasView beastCanvasPrefab = _prefabs.ui.GetCanvas(canvasPrefabID);
            BeastCanvasView view = BeastScene.InstantiateMono(this, beastCanvasPrefab, float3.zero, quaternion.identity);
            return view;
        }

        public BeastUIWindowView NewWindow(string windowPrefabID)
        {
            BeastUIWindowView windowPrefab = _prefabs.ui.GetWindow(windowPrefabID);
            BeastUIWindowView view = BeastScene.InstantiateMono(this, windowPrefab, float3.zero, quaternion.identity);
            return view;
        }
    }
}