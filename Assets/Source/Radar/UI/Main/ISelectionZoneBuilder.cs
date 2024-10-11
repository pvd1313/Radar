using System;

namespace Beast.Radar.UI
{
    public interface ISelectionZoneBuilder
    {
        Tuple<ESelectionZone,SelectableGridData> Build(
            ESelectionZone zone, UISelectable[] selectables, 
            int cols, int rows, bool isFillHorizontally, bool isLooping);
        Tuple<ESelectionZone,SelectableGridData> Build(
            ESelectionZone zone, UISelectable[] selectables, 
            bool isFillHorizontally, bool isLooping);
        Tuple<ESelectionZone,SelectableGridData> Build(
            ESelectionZone zone, UISelectable[,] selectionsGrid, bool isLooping);
        Tuple<ESelectionZone,SelectableGridData> Build(ESelectionZone zone, UISelectable selectable);
    }
}