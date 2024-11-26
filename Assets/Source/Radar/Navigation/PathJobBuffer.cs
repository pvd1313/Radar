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

        public static PathJobBuffer Create(int capacity, Allocator allocator)
        {
            return new PathJobBuffer
            {
                openList = new PriorityQueue(allocator),
                outputPath = new NativeList<int2>(allocator),
                pathNodeList = new NativeList<PathNode>(capacity, allocator),
                positionToIndexMap = new NativeHashMap<int2, int>(capacity, allocator),
                closedList = new NativeHashSet<int>(capacity, allocator),
                openSet = new NativeHashSet<int>(capacity, allocator),
            };
        }

        public void Clear()
        {
            outputPath.Clear();
            pathNodeList.Clear();
            positionToIndexMap.Clear();
            openList.Clear();
            closedList.Clear();
            openSet.Clear();
        }

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