using System.Collections.Generic;

namespace Beast.Radar.UI
{
    public interface ISelectionManager
    {
        void Set(Dictionary<ESelectionZone, SelectableGridData> windowSelectableZones);
        void Reset();
    }
}