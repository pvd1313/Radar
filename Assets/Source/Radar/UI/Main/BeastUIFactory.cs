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

        public BeastCanvasView CreateCanvas<TOwner>(TOwner owner)
        {
            BeastCanvasView prefab = _prefabs.ui.canvas;
            BeastCanvasView view = BeastScene.InstantiateMono(owner, prefab, float3.zero, quaternion.identity);
            
            return view;
        }

        public void Create<TOwner, TView>(TOwner owner, string prefabID, out TView newView) where TView : BeastUIView
        {
            TView prefab = (TView) _prefabs.ui.GetWindow(prefabID);
            
            newView = BeastScene.InstantiateMono(owner, prefab, float3.zero, quaternion.identity);
        }
    }
}