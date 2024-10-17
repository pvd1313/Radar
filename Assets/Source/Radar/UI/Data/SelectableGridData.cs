using Unity.Mathematics;

namespace Beast.Radar.UI
{
    public struct SelectableGridData
    {
        public UISelectable[,] selectables;
        public int2 currentIndex;
        public bool isLooping;
    }
}