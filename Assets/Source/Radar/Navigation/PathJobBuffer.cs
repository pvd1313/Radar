using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Radar.Navigation
{
    public struct PathJobBuffer
    {
        public NativeList<int2> outputPath;
        public NativeList<PathNode> pathNodeList;
        public NativeHashMap<int2, int> positionToIndexMap;
        public PriorityQueue openList;
        public NativeHashSet<int> closedList;
        public NativeHashSet<int> openSet;

        public void Dispose()
        {
            outputPath.Dispose();
            pathNodeList.Dispose();
            positionToIndexMap.Dispose();
            openList.Dispose();
            closedList.Dispose();
            openSet.Dispose();
        }
    }
}