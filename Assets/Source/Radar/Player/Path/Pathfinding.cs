using System.Diagnostics;
using Beast.Claw.Debug;
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

        public Pathfinding(int size)
        {
            _size = size;
        }

        public int2[] FindPathArray(int2 startPosition, int2 endPosition, NativeArray<EBiomeName> texture, out long elapsedMilliseconds)
        {
            NativeArray<int2> pathNativeArray = FindPath(startPosition, endPosition, texture, out elapsedMilliseconds);
            int2[] result = new int2[pathNativeArray.Length];
            pathNativeArray.CopyTo(result);
            pathNativeArray.Dispose();
            return result;
        }

        private NativeArray<int2> FindPath(int2 startPosition, int2 endPosition, NativeArray<EBiomeName> texture, out long elapsedMilliseconds)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            NativeList<int2> pathResultList = new NativeList<int2>(Allocator.TempJob);

            FindPathJob findPathJob = new FindPathJob
            {
                size = _size,
                startPosition = startPosition,
                endPosition = endPosition,
                pathResultList = pathResultList,
                texture = texture
            };

            JobHandle jobHandle = findPathJob.Schedule();
            jobHandle.Complete();

            stopwatch.Stop();
            elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            NativeArray<int2> pathNativeArray = new NativeArray<int2>(pathResultList.Length, Allocator.Temp);
            for (int i = 0; i < pathResultList.Length; i++)
            {
                pathNativeArray[i] = pathResultList[i];
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

            public void Execute()
            {
                int2 gridSize = new int2(size, size);
                NativeList<PathNode> pathNodeList = new NativeList<PathNode>(Allocator.Temp);
                NativeHashMap<int2, int> positionToIndexMap = new NativeHashMap<int2, int>(gridSize.x * gridSize.y, Allocator.Temp);
                NativeArray<int2> neighbourOffsetArray = new NativeArray<int2>(new[]
                {
                    new int2(-1, 0), new int2(1, 0), new int2(0, 1), new int2(0, -1),
                    new int2(-1, -1), new int2(-1, 1), new int2(1, -1), new int2(1, 1),
                }, Allocator.Temp);

                int endNodeIndex = CreateOrGetPathNode(endPosition, ref pathNodeList, ref positionToIndexMap, gridSize);

                PathNode startNode = pathNodeList[CreateOrGetPathNode(startPosition, ref pathNodeList, ref positionToIndexMap, gridSize)];
                startNode.gCost = 0;
                startNode.CalculateFCost();
                pathNodeList[startNode.index] = startNode;

                NativeMinHeap openList = new NativeMinHeap(Allocator.Temp);
                NativeList<int> closedList = new NativeList<int>(Allocator.Temp);
                openList.Enqueue(startNode.index, startNode.fCost);

                while (openList.Count > 0)
                {
                    int currentNodeIndex = openList.Dequeue();
                    PathNode currentNode = pathNodeList[currentNodeIndex];if (currentNodeIndex == endNodeIndex)
                    {
                        break;
                    }

                    closedList.Add(currentNodeIndex);

                    for (int i = 0; i < neighbourOffsetArray.Length; i++)
                    {
                        int2 neighbourPosition = new int2(currentNode.x, currentNode.y) + neighbourOffsetArray[i];
                        if (!IsPositionInsideGrid(neighbourPosition, gridSize))
                        {
                            continue;
                        }

                        int neighbourNodeIndex = CreateOrGetPathNode(neighbourPosition, ref pathNodeList, ref positionToIndexMap, gridSize);
                        if (closedList.Contains(neighbourNodeIndex))
                        {
                            continue;
                        }

                        PathNode neighbourNode = pathNodeList[neighbourNodeIndex];
                        if (!neighbourNode.isWalkable)
                        {
                            continue;
                        }

                        int tentativeGCost = currentNode.gCost + CalculateDistanceCost(new int2(currentNode.x, currentNode.y), neighbourPosition);
                        if (tentativeGCost < neighbourNode.gCost)
                        {
                            neighbourNode.cameFromNodeIndex = currentNodeIndex;
                            neighbourNode.gCost = tentativeGCost;
                            neighbourNode.CalculateFCost();
                            pathNodeList[neighbourNodeIndex] = neighbourNode;

                            if (!openList.TryUpdate(neighbourNodeIndex, neighbourNode.fCost))
                            {
                                openList.Enqueue(neighbourNodeIndex, neighbourNode.fCost);
                            }
                        }
                    }
                }

                CalculatePath(pathNodeList, pathNodeList[endNodeIndex]);

                pathNodeList.Dispose();
                positionToIndexMap.Dispose();
                neighbourOffsetArray.Dispose();
                openList.Dispose();
                closedList.Dispose();
            }

            private int CreateOrGetPathNode(int2 position, ref NativeList<PathNode> pathNodeList, ref NativeHashMap<int2, int> positionToIndexMap, int2 gridSize)
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
                    isWalkable = texture[CalculateIndex(position.x, position.y, gridSize.x)] != EBiomeName.Deep,
                    cameFromNodeIndex = -1
                });

                pathNodeList[newIndex].CalculateFCost();
                return newIndex;
            }

            private void CalculatePath(NativeList<PathNode> pathNodes, PathNode endNode)
            {
                if (endNode.cameFromNodeIndex == -1) return;

                while (endNode.cameFromNodeIndex != -1)
                {
                    pathResultList.Add(new int2(endNode.x, endNode.y));
                    endNode = pathNodes[endNode.cameFromNodeIndex];
                }
            }

            private bool IsPositionInsideGrid(int2 gridPosition, int2 gridSize)
            {
                return gridPosition.x >= 0 && gridPosition.y >= 0 && gridPosition.x < gridSize.x && gridPosition.y < gridSize.y;
            }

            private int CalculateIndex(int x, int y, int gridWidth)
            {
                return x + y * gridWidth;
            }private int CalculateDistanceCost(int2 startPosition, int2 targetPosition)
            {
                int xDistance = math.abs(startPosition.x - targetPosition.x);
                int yDistance = math.abs(startPosition.y - targetPosition.y);
                int remaining = math.abs(xDistance - yDistance);
                return MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
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
            public bool isWalkable;

            public void CalculateFCost()
            {
                fCost = gCost + hCost;
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

            public bool TryUpdate(int item, int newPriority)
            {
                int index = items.IndexOf(item);
                if (index >= 0)
                {
                    if (priorities[index] > newPriority)
                    {
                        priorities[index] = newPriority;
                        HeapifyUp(index);
                        return true;
                    }
                    return false;
                }
                return false;
            }

            public int Dequeue()
            {
                int item = items[0];
                items[0] = items[items.Length - 1];
                priorities[0] = priorities[priorities.Length - 1];
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
    }
}