using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Radar.Navigation
{
    public struct NavigationPathSearchBounds
    {
        public int2 startCell;
        public int2 finishCell;

        public int terrainSize;
        
        public float cell8KMSize;
        public NativeSlice<float2> include8KMWorldPoints;
    }
}