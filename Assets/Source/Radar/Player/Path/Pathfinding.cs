using System.Diagnostics;
using Beast.Radar.Terrain;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Beast.Radar.Player.Path
{
    public class Pathfinding
    {
        private const int MOVE_STRAIGHT_COST = 1;
        private const int MOVE_DIAGONAL_COST = 1;

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

                int endNodeIndex = CreateOrGetPathNode(endPosition, ref pathNodeList, ref positionToIndexMap);
                
                PathNode startNode =
                    pathNodeList[
                        CreateOrGetPathNode(startPosition, ref pathNodeList, ref positionToIndexMap)];
               
                startNode.gCost = 0;
                startNode.CalculateFCost();
                pathNodeList[startNode.index] = startNode;

                NativeMinHeap openList = new(Allocator.Temp);
                NativeHashSet<int> closedList = new(gridSize.x * gridSize.y, Allocator.Temp);
                NativeHashSet<int> openSet = new(gridSize.x * gridSize.y, Allocator.Temp);

                openList.Enqueue(startNode.index, startNode.fCost);
                openSet.Add(startNode.index);

                while (openList.Count > 0)
                {
                    int currentNodeIndex = openList.Dequeue();
                   
                    openSet.Remove(currentNodeIndex);
                    
                    if (currentNodeIndex == endNodeIndex)
                    {
                        break;
                    }

                    closedList.Add(currentNodeIndex);

                    foreach (int2 offset in neighbourOffsets)
                    {
                        int2 neighbourPosition =
                            new int2(pathNodeList[currentNodeIndex].x, pathNodeList[currentNodeIndex].y) + offset;
                        
                        if (!IsPositionInsideGrid(neighbourPosition, gridSize))
                        {
                            continue;
                        }

                        int neighbourNodeIndex = CreateOrGetPathNode(neighbourPosition, ref pathNodeList,
                            ref positionToIndexMap);
                        if (closedList.Contains(neighbourNodeIndex))
                        {
                            continue;
                        }

                        int tentativeGCost =
                            CalculateTentativeGCost(pathNodeList, currentNodeIndex, neighbourNodeIndex);
                        
                        PathNode neighbourNode = pathNodeList[neighbourNodeIndex];

                        if (tentativeGCost < neighbourNode.gCost)
                        {
                            neighbourNode.cameFromNodeIndex = currentNodeIndex;
                            neighbourNode.gCost = tentativeGCost;
                            neighbourNode.CalculateFCost();
                            pathNodeList[neighbourNodeIndex] = neighbourNode;

                            if (!openSet.Contains(neighbourNodeIndex))
                            {
                                openList.Enqueue(neighbourNodeIndex, neighbourNode.fCost);
                                openSet.Add(neighbourNodeIndex);
                            }
                        }
                    }
                }

                CalculatePath(pathNodeList, pathNodeList[endNodeIndex]);

                pathNodeList.Dispose();
                positionToIndexMap.Dispose();
                openList.Dispose();
                closedList.Dispose();
                openSet.Dispose(); 
            }

            private int CalculateTentativeGCost(NativeList<PathNode> pathNodeList, int currentNodeIndex,
                int neighbourNodeIndex)
            {
                PathNode currentNode = pathNodeList[currentNodeIndex];
                int2 neighbourPosition = new(pathNodeList[neighbourNodeIndex].x, pathNodeList[neighbourNodeIndex].y);

                int textureIndex = neighbourPosition.x + neighbourPosition.y * size;
                byte passability = tileSpeedModifiers.GetByte((byte)texture[textureIndex]);

                int movementCost = passability *
                                   CalculateDistanceCost(new int2(currentNode.x, currentNode.y), neighbourPosition);
              
                return currentNode.gCost + movementCost;
            }

            private bool IsPositionInsideGrid(int2 gridPosition, int2 gridSize)
            {
                return gridPosition.x >= 0 && gridPosition.y >= 0 && gridPosition.x < gridSize.x &&
                       gridPosition.y < gridSize.y;
            }

            private int CreateOrGetPathNode(int2 position, ref NativeList<PathNode> pathNodeList,
                ref NativeHashMap<int2, int> positionToIndexMap)
            {
                if (positionToIndexMap.TryGetValue(position, out int index))
                {
                    return index;
                }

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

            private int CalculateDistanceCost(int2 start, int2 end)
            {
                int xDistance = math.abs(start.x - end.x);
                int yDistance = math.abs(start.y - end.y);
                int remaining = math.abs(xDistance - yDistance);
                return MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
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
        }

        private struct NativeMinHeap : System.IDisposable
        {
            public int Count => _items.Length;
            
            private NativeList<int> _items;
            private NativeList<int> _priorities;
            
            public NativeMinHeap(Allocator allocator)
            {
                _items = new NativeList<int>(allocator);
                _priorities = new NativeList<int>(allocator);
            }

            public void Enqueue(int item, int priority)
            {
                _items.Add(item);
                _priorities.Add(priority);
                HeapifyUp(_items.Length - 1);
            }

            public int Dequeue()
            {
                int item = _items[0];
                _items[0] = _items[^1];
                _priorities[0] = _priorities[^1];
                _items.RemoveAt(_items.Length - 1);
                _priorities.RemoveAt(_priorities.Length - 1);
                HeapifyDown(0);
                return item;
            }

            private void HeapifyUp(int index)
            {
                while (index > 0)
                {
                    int parentIndex = (index - 1) / 2;
                    if (_priorities[parentIndex] <= _priorities[index]) break;

                    Swap(index, parentIndex);
                    index = parentIndex;
                }
            }

            private void HeapifyDown(int index)
            {
                while (index < _items.Length)
                {
                    int smallest = index;
                    int left = 2 * index + 1;
                    int right = 2 * index + 2;

                    if (left < _items.Length && _priorities[left] < _priorities[smallest]) smallest = left;
                    if (right < _items.Length && _priorities[right] < _priorities[smallest]) smallest = right;

                    if (smallest == index) break;

                    Swap(index, smallest);
                    index = smallest;
                }
            }

            private void Swap(int indexA, int indexB)
            {
                (_items[indexA], _items[indexB]) = (_items[indexB], _items[indexA]);
                (_priorities[indexA], _priorities[indexB]) = (_priorities[indexB], _priorities[indexA]);
            }

            public void Dispose()
            {
                _items.Dispose();
                _priorities.Dispose();
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