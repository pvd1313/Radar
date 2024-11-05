using System.Diagnostics;
using Beast.Radar.Terrain;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
 
namespace Beast.Radar.Player.Path
{
    public class Pathfinding
    {
        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;
 
        private readonly int _size;
        private readonly ByteArray16 _tileSpeedModifiers;
        private readonly NativeArray<EBiomeName> _texture;
        private NativeArray<int2> _neighbourOffsets;
        
        public Pathfinding(int size, ByteArray16 tileSpeedModifiers, NativeArray<EBiomeName> texture)
        {
            _size = size;
            _tileSpeedModifiers = tileSpeedModifiers;
            _texture = texture;
 
            _neighbourOffsets = new NativeArray<int2>(new[]
            {
                new int2(-1, 0), new int2(1, 0), new int2(0, 1), new int2(0, -1),
                new int2(-1, -1), new int2(-1, 1), new int2(1, -1), new int2(1, 1),
            }, Allocator.Persistent);
        }
 
        public void Dispose()
        {
            _neighbourOffsets.Dispose();
        }
 
        public int2[] FindPathArray(int2 startPosition, int2 endPosition, out long elapsedMilliseconds)
        {
            NativeArray<int2> pathNativeArray = FindPath(startPosition, endPosition, out elapsedMilliseconds);
            int2[] result = new int2[pathNativeArray.Length];
            pathNativeArray.CopyTo(result);
            pathNativeArray.Dispose();
            return result;
        }
 
        private NativeArray<int2> FindPath(int2 startPosition, int2 endPosition, out long elapsedMilliseconds)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
 
            NativeList<int2> pathResultList = new(Allocator.TempJob);
 
            FindPathJob findPathJob = new()
            {
                size = _size,
                startPosition = startPosition,
                endPosition = endPosition,
                pathResultList = pathResultList,
                texture = _texture,
                tileSpeedModifiers = _tileSpeedModifiers,
                neighbourOffsets = _neighbourOffsets
            };
 
            JobHandle jobHandle = findPathJob.Schedule();
            jobHandle.Complete();
 
            stopwatch.Stop();
            elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
 
            NativeArray<int2> pathNativeArray = new(pathResultList.Length, Allocator.TempJob);
 
            for (int index = 0; index < pathResultList.Length; index++)
            {
                pathNativeArray[index] = pathResultList[index];
            }
 
            pathResultList.Dispose();
 
            return pathNativeArray;
        }
 
        [BurstCompatible]
        private struct FindPathJob : IJob
        {
            public int size;
            public int2 startPosition;
            public int2 endPosition;
            public NativeList<int2> pathResultList;
            public NativeArray<EBiomeName> texture;
            public ByteArray16 tileSpeedModifiers;
            [ReadOnly] public NativeArray<int2> neighbourOffsets;
 
            public void Execute()
            {
                int2 gridSize = new int2(size, size);
                NativeList<PathNode> pathNodeList = new(Allocator.Temp);
                NativeHashMap<int2, int> positionToIndexMap = new(gridSize.x * gridSize.y, Allocator.Temp);
 
                int endNodeIndex = CreateOrGetPathNode(endPosition, ref pathNodeList, ref positionToIndexMap, gridSize);
 
                PathNode startNode = pathNodeList[CreateOrGetPathNode(startPosition, ref pathNodeList, ref positionToIndexMap, gridSize)];
                startNode.gCost = 0;
                startNode.CalculateFCost();
                pathNodeList[startNode.index] = startNode;
 
                NativeMinHeap openList = new(Allocator.Temp);
                NativeHashSet<int> closedList = new(gridSize.x * gridSize.y, Allocator.Temp); 
                openList.Enqueue(startNode.index, startNode.fCost);
 
                while (openList.Count > 0)
                {
                    int currentNodeIndex = openList.Dequeue();
                    if (currentNodeIndex == endNodeIndex)
                    {
                        break;
                    }
 
                    closedList.Add(currentNodeIndex);
 
                    for (int i = 0; i < neighbourOffsets.Length; i++)
                    {
                        int2 neighbourPosition =
                            new int2(pathNodeList[currentNodeIndex].x, pathNodeList[currentNodeIndex].y) +
                            neighbourOffsets[i];
                        if (!IsPositionInsideGrid(neighbourPosition, gridSize))
                        {
                            continue;
                        }
 
                        int neighbourNodeIndex = CreateOrGetPathNode(neighbourPosition, 
                            ref pathNodeList,
                            ref positionToIndexMap, gridSize);

                        if (closedList.Contains(neighbourNodeIndex))
                        {
                            continue;
                        }
 
                        PathNode neighbourNode = pathNodeList[neighbourNodeIndex];

                        int textureIndex =neighbourPosition.x + neighbourPosition.y * gridSize.x;
                        byte passability = tileSpeedModifiers.GetByte((byte)texture[textureIndex]);

                        int tentativeGCost = pathNodeList[currentNodeIndex].gCost +
                                             passability * CalculateDistanceCost(
                                                 new int2(pathNodeList[currentNodeIndex].x,
                                                     pathNodeList[currentNodeIndex].y), neighbourPosition);
 
                        if (tentativeGCost < neighbourNode.gCost)
                        {
                            neighbourNode.cameFromNodeIndex = currentNodeIndex;
                            neighbourNode.gCost = tentativeGCost;
                            neighbourNode.CalculateFCost();
                            pathNodeList[neighbourNodeIndex] = neighbourNode;
                            openList.Enqueue(neighbourNodeIndex, neighbourNode.fCost);
                        }
                    }
                }
 
                CalculatePath(pathNodeList, pathNodeList[endNodeIndex]);
 
                pathNodeList.Dispose();
                positionToIndexMap.Dispose();
                openList.Dispose();
                closedList.Dispose();
            }
 
            private void CalculatePath(NativeList<PathNode> pathNodes, PathNode endNode)
            {
                if (endNode.cameFromNodeIndex == -1) return;
 
                while (endNode.cameFromNodeIndex != -1)
                {
                    pathResultList.Add(new int2(endNode.x, endNode.y));
                    endNode = pathNodes[endNode.cameFromNodeIndex];
                }
 
                pathResultList.Add(new int2(endNode.x, endNode.y));
            }
            
            private int CreateOrGetPathNode(int2 position, ref NativeList<PathNode> pathNodeList,
                ref NativeHashMap<int2, int> positionToIndexMap, int2 gridSize)
            {
                if (positionToIndexMap.TryGetValue(position, out int index))
                    return index;
 
                int newIndex = pathNodeList.Length;
                positionToIndexMap[position] = newIndex;
 
                pathNodeList.Add(new PathNode
                {
                    x = position.x,
                    y = position.y,
                    index = newIndex,
                    gCost = int.MaxValue,
                    hCost = CalculateDistanceCost(position, endPosition),
                    cameFromNodeIndex = -1
                });
 
                pathNodeList[newIndex].CalculateFCost();
                return newIndex;
            }
 
            private bool IsPositionInsideGrid(int2 gridPosition, int2 gridSize)
            {
                return gridPosition.x >= 0 && gridPosition.y >= 0 && gridPosition.x < gridSize.x &&
                       gridPosition.y < gridSize.y;
            }

            private int CalculateDistanceCost(int2 start, int2 end)
            {
                int xDistance = math.abs(start.x - end.x);
                int yDistance = math.abs(start.y - end.y);
                int remaining = math.abs(xDistance - yDistance);
                return MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
            }
        }
 
        private struct NativeMinHeap : System.IDisposable
        {
            private NativeList<int> items;
            private NativeList<int> priorities;
 
            public NativeMinHeap(Allocator allocator)
            {
                items = new NativeList<int>(allocator);
                priorities = new NativeList<int>(allocator);
            }
 
            public int Count => items.Length;
 
            public void Enqueue(int item, int priority)
            {
                items.Add(item);
                priorities.Add(priority);
                HeapifyUp(items.Length - 1);
            }

            public int Dequeue()
            {
                int item = items[0];
                items[0] = items[^1];
                priorities[0] = priorities[^1];
                items.RemoveAt(items.Length - 1);
                priorities.RemoveAt(priorities.Length - 1);
                HeapifyDown(0);
                return item;
            }
 
            private void HeapifyUp(int index)
            {
                while (index > 0)
                {
                    int parentIndex = (index - 1) / 2;
                    if (priorities[parentIndex] <= priorities[index]) break;
 
                    Swap(index, parentIndex);
                    index = parentIndex;
                }
            }
 
            private void HeapifyDown(int index)
            {
                while (index < items.Length)
                {
                    int smallest = index;
                    int left = 2 * index + 1;
                    int right = 2 * index + 2;
 
                    if (left < items.Length && priorities[left] < priorities[smallest]) smallest = left;
                    if (right < items.Length && priorities[right] < priorities[smallest]) smallest = right;
 
                    if (smallest == index) break;
 
                    Swap(index, smallest);
                    index = smallest;
                }
            }
 
            private void Swap(int indexA, int indexB)
            {
                (items[indexA], items[indexB]) = (items[indexB], items[indexA]);
                (priorities[indexA], priorities[indexB]) = (priorities[indexB], priorities[indexA]);
            }
 
            public void Dispose()
            {
                items.Dispose();
                priorities.Dispose();
            }
        }
 
        private struct PathNode
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
}