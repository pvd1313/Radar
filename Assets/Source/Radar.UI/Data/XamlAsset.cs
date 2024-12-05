using System;
using Beast.Claw.UI;
using Noesis;

namespace Beast.Radar.UI
{
    [Serializable]
    public struct XamlAsset
    {
        public NoesisXaml asset;

        public void Instantiate<T>(out T control) where T : BeastControl
        {
            asset.Instantiate(out control);
        }
        public ResourceDictionary LoadResourceDictionary()
        {
            return asset.Load() as ResourceDictionary;
        }
    }
}