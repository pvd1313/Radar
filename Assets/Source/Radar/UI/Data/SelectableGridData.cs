using Unity.Mathematics;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public struct SelectableGridData
    {
        public Selectable[,] selectables;
        public int2 currentIndex;
        public bool isLooping;
    }
}