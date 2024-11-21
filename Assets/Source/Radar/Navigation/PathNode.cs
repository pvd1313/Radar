using Unity.Mathematics;

namespace Beast.Radar.Navigation
{
    public struct PathNode
    {
        public int2 position;
        public int index;
        public int gCost;
        public int hCost;
        public int fCost;
        public int cameFromNodeIndex;

        public void CalculateFCost()
        {
            fCost = gCost + hCost;
        }
    }
}