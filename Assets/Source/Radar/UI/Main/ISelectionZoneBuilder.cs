using Unity.Mathematics;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public interface ISelectionZoneBuilder
    {
        public SelectableGridData Build(Selectable[] selectables, int cols, int rows, bool isFillHorizontally, bool isLooping);
        SelectableGridData Build(Selectable[] selectables, bool isFillHorizontally, bool isLooping);
        SelectableGridData Build(Selectable[,] selectables, bool isLooping);
        SelectableGridData Build(Selectable selectables);
    }
}