using System.Runtime.CompilerServices;
using Beast.Claw.Mathematics;
using Beast.Radar.Terrain;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Beast.Radar.Player.Path
{
    [BurstCompile]
    public struct FindPathJob : IJob
    {
        private const int MOVE_STRAIGHT_COST = 1;
        private const int MOVE_DIAGONAL_COST = 1;
        
        public int textureSizeX;
        public int2 startPosition;
        public int2 endPosition;
        public NativeArray<EBiomeName> texture;
        public ByteArray16 tileCosts;
        
        public NativeList<int2> outputPath;

        public void Execute()
        {
            outputPath.Clear();
            
            int2 gridSizeXY = new (textureSizeX, textureSizeX);
            NativeList<PathNode> pathNodeList = new(Allocator.Temp);
            NativeHashMap<int2, int> positionToIndexMap = new(gridSizeXY.x * gridSizeXY.y, Allocator.Temp);

            int endNodeIndex = _createOrGetPathNode(endPosition, ref pathNodeList, ref positionToIndexMap);
                
            PathNode startNode =
                pathNodeList[_createOrGetPathNode(startPosition, ref pathNodeList, ref positionToIndexMap)];
               
            startNode.gCost = 0;
            startNode.CalculateFCost();
            pathNodeList[startNode.index] = startNode;

            PriorityQueue openList = new(Allocator.Temp);
            NativeHashSet<int> closedList = new(gridSizeXY.x * gridSizeXY.y, Allocator.Temp);
            NativeHashSet<int> openSet = new(gridSizeXY.x * gridSizeXY.y, Allocator.Temp);

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

                for (int i = 0; i < NeighbourOffsets.COUNT; i++)
                {
                    int2 offset = NeighbourOffsets.Get(i);
                    
                    PathNode currentNode = pathNodeList[currentNodeIndex];
                    int2 neighbourPosition = new int2(currentNode.x, currentNode.y) + offset;
                        
                    if (!_isPositionInsideGrid(neighbourPosition, gridSizeXY))
                    {
                        continue;
                    }

                    int neighbourNodeIndex = _createOrGetPathNode(
                        neighbourPosition, 
                        ref pathNodeList,
                        ref positionToIndexMap);
                    
                    if (closedList.Contains(neighbourNodeIndex))
                    {
                        continue;
                    }

                    int tentativeGCost = _calculateTentativeGCost(pathNodeList, currentNodeIndex, neighbourNodeIndex);
                        
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

            _calcPath(pathNodeList, pathNodeList[endNodeIndex]);

            pathNodeList.Dispose();
            positionToIndexMap.Dispose();
            openList.Dispose();
            closedList.Dispose();
            openSet.Dispose(); 
        }

        private int _calculateTentativeGCost(
            NativeList<PathNode> pathNodeList, 
            int currentNodeIndex,
            int neighbourNodeIndex)
        {
            PathNode currentNode = pathNodeList[currentNodeIndex];
            int2 neighbourPosition = new(pathNodeList[neighbourNodeIndex].x, pathNodeList[neighbourNodeIndex].y);

            int textureIndex = neighbourPosition.x + neighbourPosition.y * textureSizeX;
            int tileCost = tileCosts.GetByte((byte)texture[textureIndex]);

            int distanceCost = _calcDistanceCost(new int2(currentNode.x, currentNode.y), neighbourPosition);
            int movementCost = tileCost * distanceCost;
              
            return currentNode.gCost + movementCost;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool _isPositionInsideGrid(int2 gridPosition, int2 gridSize)
        {
            return gridPosition.x >= 0 && 
                   gridPosition.y >= 0 && 
                   gridPosition.x < gridSize.x &&
                   gridPosition.y < gridSize.y;
        }

        private int _createOrGetPathNode(
            int2 position, 
            ref NativeList<PathNode> pathNodeList,
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
                hCost = _calcDistanceCost(position, endPosition),
                cameFromNodeIndex = -1
            });

            pathNodeList[newIndex].CalculateFCost();
            
            return newIndex;
        }

        private int _calcDistanceCost(int2 start, int2 end)
        {
            return (int)BMath.DistanceSq(start, end);

            // int xDistance = BMath.Abs(start.x - end.x);
            // int yDistance = BMath.Abs(start.y - end.y);
            // int remaining = BMath.Abs(xDistance - yDistance);
            //
            // return MOVE_DIAGONAL_COST * BMath.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }

        private void _calcPath(NativeList<PathNode> pathNodes, PathNode endNode)
        {
            if (endNode.cameFromNodeIndex == -1)
            {
                return;
            }

            while (endNode.cameFromNodeIndex != -1)
            {
                outputPath.Add(new int2(endNode.x, endNode.y));
                endNode = pathNodes[endNode.cameFromNodeIndex];
            }

            outputPath.Add(new int2(endNode.x, endNode.y));
        }
    }
}