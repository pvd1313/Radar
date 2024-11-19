using System;
using Beast.Claw.UI;

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
    }
}