using Beast.Claw.Common;
using Unity.Mathematics;

namespace Beast.Radar.UI
{
    public class UIFactory : IUIFactory
    {
        private readonly PrefabLibrary _prefabs;

        public UIFactory(PrefabLibrary prefabs)
        {
            _prefabs = prefabs;
        }

        public void Create<TOwner, TView>(TOwner owner, string prefabID, out TView newView) where TView : UIView
        {
            TView prefab = (TView) _prefabs.ui.GetWindow(prefabID);
            
            newView = BScene.InstantiateMono(owner, prefab, float3.zero, quaternion.identity);
        }
    }
}