using System.Diagnostics;
using Beast.Claw.Debug;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Player.Path
{
    public class Pathfinding : MonoBehaviour
    {
        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;

        private void Start()
        {
            JobHandle jobHandle = new JobHandle();
            FindPathJob findPathJob = new FindPathJob
            {
                startPosition = int2.zero,
                endPosition = new int2(4095, 4095)
            };
            findPathJob.Schedule();

            jobHandle.Complete();
        }

        [BurstCompatible]
        private struct FindPathJob : IJob
        {
            public int2 startPosition;
            public int2 endPosition;
            
            public void Execute()
            {
                Stopwatch stopwatch = new();
                stopwatch.Start();
                int2 gridSize = new int2(4096, 4096);

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

                if (pathNodeArray[endNodeIndex].cameFromNodeIndex == -1)
                {
                    BDebug.LogWarning(this, "Did not find a path");
                }

                pathNodeArray.Dispose();
                neighbourOffsetArray.Dispose();
                openList.Dispose();
                closedList.Dispose();
                stopwatch.Stop();
                
                BDebug.LogInfo(this, $"{stopwatch.ElapsedMilliseconds} ms");
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

            private NativeArray<int2> CalculatePath(NativeArray<PathNode> pathNodes, PathNode endNode)
            {
                if (endNode.cameFromNodeIndex == -1) return new NativeArray<int2>(0, Allocator.Temp);
                NativeList<int2> path = new(Allocator.Temp);
                while (endNode.cameFromNodeIndex != -1)
                {
                    path.Add(new int2(endNode.x, endNode.y));
                    endNode = pathNodes[endNode.cameFromNodeIndex];
                }
                return path.ToArray(Allocator.Temp);
            }

            private void LogPath(NativeArray<int2> path)
            {
                foreach (var position in path) BDebug.LogInfo(this, position.ToString());
                path.Dispose();
                BDebug.LogInfo(this, "Found a path");
            }

            private bool IsPositionInsideGrid(int2 gridPosition, int2 gridSize) =>
                gridPosition.x >= 0 && gridPosition.y >= 0 && gridPosition.x < gridSize.x && gridPosition.y < gridSize.y;

            private int CalculateIndex(int x, int y, int gridWidth) => x + y * gridWidth;

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
