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

        public BeastCanvasView NewCanvas<TOwner>(TOwner owner)
        {
            BeastCanvasView prefab = _prefabs.ui.canvas;
            BeastCanvasView view = BeastScene.InstantiateMono(owner, prefab, float3.zero, quaternion.identity);
            
            return view;
        }

        public TView NewWindow<TOwner, TView>(TOwner owner, string windowPrefabID) where TView : BeastUIView
        {
            TView prefab = (TView) _prefabs.ui.GetWindow(windowPrefabID);
            TView view = BeastScene.InstantiateMono(owner, prefab, float3.zero, quaternion.identity);
            
            return view;
        }
    }
}