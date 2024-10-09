using Unity.Mathematics;
using UnityEditor;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public interface ISelectionZoneBuilder
    {
        public SelectableGridData Build(Selectable[] selectables, int2 size, bool isFillHorizontally, bool isLooping);
        SelectableGridData Build(Selectable[] selectables, bool isFillHorizontally, bool isLooping);
    }
}