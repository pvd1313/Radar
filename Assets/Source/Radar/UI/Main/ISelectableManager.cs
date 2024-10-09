using System.Collections.Generic;

namespace Beast.Radar.UI
{
    public interface ISelectableManager
    {
        void Set(Dictionary<ESelectionZone, SelectableGridData> windowSelectableZones);
        void Reset();
    }
}