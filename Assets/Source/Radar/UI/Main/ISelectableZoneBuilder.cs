using System;

namespace Beast.Radar.UI
{
    public interface ISelectableZoneBuilder
    {
        Tuple<ESelectionZone,SelectableGridData> Build(
            ESelectionZone zone, UISelectable[] selectables, 
            int cols, int rows, bool isFillHorizontally, bool isLooping);
        Tuple<ESelectionZone,SelectableGridData> Build(
            ESelectionZone zone, UISelectable[] selectables, 
            bool isFillHorizontally, bool isLooping);
        Tuple<ESelectionZone,SelectableGridData> Build(
            ESelectionZone zone, UISelectable[,] selectableGrid, bool isLooping);
        Tuple<ESelectionZone,SelectableGridData> Build(ESelectionZone zone, UISelectable selectable);
    }
}