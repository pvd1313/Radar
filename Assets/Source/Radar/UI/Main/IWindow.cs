using System.Collections.Generic;

namespace Beast.Radar.UI
{
    public interface IWindow
    {
        public Dictionary<ESelectionZone, SelectableGridData> Selections { get; }
        public void Open();
        public void Close();
    }
}