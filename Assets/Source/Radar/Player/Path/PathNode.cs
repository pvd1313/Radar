namespace Beast.Radar.Player.Path
{
    public struct PathNode
    {
        public int x;
        public int y;
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