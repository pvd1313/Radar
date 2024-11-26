using Unity.Mathematics;

namespace Beast.Radar.Navigation
{
    public struct PathNode
    {
        public int2 position;
        public int index;
        public float gCost;
        public float hCost;
        public float fCost;
        public int cameFromNodeIndex;

        public void CalculateFCost()
        {
            fCost = gCost + hCost;
        }
    }
}