using Unity.Mathematics;
using UnityEditor;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public interface ISelectionZoneBuilder
    {
        SelectableGridData Build(Selectable[] selectables, int2 size, bool isLooping);
    }
}