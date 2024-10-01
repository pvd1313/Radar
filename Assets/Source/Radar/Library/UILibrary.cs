using System.Linq;
using Beast.Claw.Common;
using Beast.Radar.UI.Panels.Base;

namespace Beast.Radar
{
    public class UILibrary: BeastLibrary
    {
        public BView[] views;

        public bool TryGetViewPrefab<T>(out T viewPrefab) where T : BView
        {
            viewPrefab = views.FirstOrDefault(v => v is T) as T;
            return viewPrefab != null;
        }
    }
}