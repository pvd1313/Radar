using System;
using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Radar.Navigation
{
    public struct PathJobBuffer : IDisposable
    {
        public NativeList<int2> outputPathReversed;
        public NativeList<PathNode> pathNodeList;
        public NativeHashMap<int2, int> positionToIndexMap;
        public NativePriorityQueue<int> openList;
        public NativeHashSet<int> closedList;
        public NativeHashSet<int> openSet;

        public static PathJobBuffer Create(int capacity, Allocator allocator)
        {
            return new PathJobBuffer
            {
                openList = new NativePriorityQueue<int>(capacity, allocator),
                outputPathReversed = new NativeList<int2>(capacity, allocator),
                pathNodeList = new NativeList<PathNode>(capacity, allocator),
                positionToIndexMap = new NativeHashMap<int2, int>(capacity, allocator),
                closedList = new NativeHashSet<int>(capacity, allocator),
                openSet = new NativeHashSet<int>(capacity, allocator),
            };
        }

        public void Clear()
        {
            outputPathReversed.Clear();
            pathNodeList.Clear();
            positionToIndexMap.Clear();
            openList.Clear();
            closedList.Clear();
            openSet.Clear();
        }

        public void Dispose()
        {
            outputPathReversed.Dispose();
            pathNodeList.Dispose();
            positionToIndexMap.Dispose();
            openList.Dispose();
            closedList.Dispose();
            openSet.Dispose();
        }
    }
}