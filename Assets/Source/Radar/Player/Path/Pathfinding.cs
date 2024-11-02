using System.Diagnostics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Beast.Radar.Player.Path
{
    public class Pathfinding 
    {
        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;

        private int _size;

        public Pathfinding(int size)
        {
            _size = size;
        }
        
        public NativeArray<int2> FindPathNative(int2 startPosition, int2 endPosition, out long elapsedMilliseconds)
        {
            return FindPath(startPosition, endPosition, true, out elapsedMilliseconds);
        }

        public int2[] FindPathArray(int2 startPosition, int2 endPosition, out long elapsedMilliseconds)
        {
            NativeArray<int2> pathNativeArray = FindPath(startPosition, endPosition, true, out elapsedMilliseconds);
            int2[] result = new int2[pathNativeArray.Length];
            pathNativeArray.CopyTo(result); 
            pathNativeArray.Dispose();
            return result;
        }

        private NativeArray<int2> FindPath(int2 startPosition, int2 endPosition, bool useNativeArray, out long elapsedMilliseconds)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            NativeList<int2> pathResultList = new NativeList<int2>(Allocator.TempJob);

            FindPathJob findPathJob = new FindPathJob
            {
                size = _size,
                startPosition = startPosition,
                endPosition = endPosition,
                pathResultList = pathResultList
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

            public void Execute()
            {
                int2 gridSize = new int2(size, size);
                NativeArray<PathNode> pathNodeArray = new(gridSize.x * gridSize.y, Allocator.Temp);
                NativeArray<int2> neighbourOffsetArray = new NativeArray<int2>(new[]
                {
                    new int2(-1, 0), new int2(1, 0), new int2(0, 1), new int2(0, -1),
                    new int2(-1, -1), new int2(-1, 1), new int2(1, -1), new int2(1, 1),
                }, Allocator.Temp);

                int endNodeIndex = CalculateIndex(endPosition.x, endPosition.y, gridSize.x);
                InitializePathNodes(pathNodeArray, gridSize, endPosition);

                PathNode startNode = pathNodeArray[CalculateIndex(startPosition.x, startPosition.y, gridSize.x)];
                startNode.gCost = 0;
                startNode.CalculateFCost();
                pathNodeArray[startNode.index] = startNode;

                NativeList<int> openList = new(Allocator.Temp);
                NativeList<int> closedList = new(Allocator.Temp);
                openList.Add(startNode.index);

                while (openList.Length > 0)
                {
                    int currentNodeIndex = GetLowestCostFNodeIndex(openList, pathNodeArray);
                    PathNode currentNode = pathNodeArray[currentNodeIndex];
                    if (currentNodeIndex == endNodeIndex) break;

                    openList.RemoveAtSwapBack(openList.IndexOf(currentNodeIndex));
                    closedList.Add(currentNodeIndex);

                    for (int i = 0; i < neighbourOffsetArray.Length; i++)
                    {
                        int2 neighbourPosition = new int2(currentNode.x, currentNode.y) + neighbourOffsetArray[i];
                        if (!IsPositionInsideGrid(neighbourPosition, gridSize)) continue;

                        int neighbourNodeIndex = CalculateIndex(neighbourPosition.x, neighbourPosition.y, gridSize.x);
                        if (closedList.Contains(neighbourNodeIndex)) continue;

                        PathNode neighbourNode = pathNodeArray[neighbourNodeIndex];
                        if (!neighbourNode.isWalkable) continue;

                        int tentativeGCost = currentNode.gCost + CalculateDistanceCost(new int2(currentNode.x, currentNode.y), neighbourPosition);
                        if (tentativeGCost < neighbourNode.gCost)
                        {
                            neighbourNode.cameFromNodeIndex = currentNodeIndex;
                            neighbourNode.gCost = tentativeGCost;
                            neighbourNode.CalculateFCost();
                            pathNodeArray[neighbourNodeIndex] = neighbourNode;

                            if (!openList.Contains(neighbourNode.index)) openList.Add(neighbourNode.index);
                        }
                    }
                }

                CalculatePath(pathNodeArray, pathNodeArray[endNodeIndex]);

                pathNodeArray.Dispose();
                neighbourOffsetArray.Dispose();
                openList.Dispose();
                closedList.Dispose();
            }

            private void InitializePathNodes(NativeArray<PathNode> pathNodeArray, int2 gridSize, int2 endPosition)
            {
                for (int x = 0; x < gridSize.x; x++)
                {
                    for (int y = 0; y < gridSize.y; y++)
                    {
                        int index = CalculateIndex(x, y, gridSize.x);
                        pathNodeArray[index] = new PathNode
                        {
                            x = x,
                            y = y,
                            index = index,
                            gCost = int.MaxValue,
                            hCost = CalculateDistanceCost(new int2(x, y), endPosition),
                            isWalkable = true,
                            cameFromNodeIndex = -1
                        };
                        pathNodeArray[index].CalculateFCost();
                    }
                }
            }

            private void CalculatePath(NativeArray<PathNode> pathNodes, PathNode endNode)
            {
                if (endNode.cameFromNodeIndex == -1)
                {
                    return;
                }

                while (endNode.cameFromNodeIndex != -1)
                {
                    pathResultList.Add(new int2(endNode.x, endNode.y));
                    endNode = pathNodes[endNode.cameFromNodeIndex];
                }
            }

            private bool IsPositionInsideGrid(int2 gridPosition, int2 gridSize)
            {
                return gridPosition.x >= 0 && gridPosition.y >= 0 && gridPosition.x < gridSize.x &&
                       gridPosition.y < gridSize.y;
            }

            private int CalculateIndex(int x, int y, int gridWidth)
            {
                return x + y * gridWidth;
            }

            private int CalculateDistanceCost(int2 startPosition, int2 targetPosition)
            {
                int xDistance = math.abs(startPosition.x - targetPosition.x);
                int yDistance = math.abs(startPosition.y - targetPosition.y);
                int remaining = math.abs(xDistance - yDistance);
                return MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
            }

            private int GetLowestCostFNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodes)
            {
                int lowestCostIndex = openList[0];
                int lowestCost = pathNodes[lowestCostIndex].fCost;
                for (int i = 1; i < openList.Length; i++)
                {
                    int nodeIndex = openList[i];
                    int nodeFCost = pathNodes[nodeIndex].fCost;
                    if (nodeFCost < lowestCost)
                    {
                        lowestCost = nodeFCost;
                        lowestCostIndex = nodeIndex;
                    }
                }
                return lowestCostIndex;
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
    }
}
