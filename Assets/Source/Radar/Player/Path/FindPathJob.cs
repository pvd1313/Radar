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
        private const int MOVE_DIAGONAL_COST = 2;

        public int textureSizeX;
        public int2 startPosition;
        public int2 endPosition;
        public NativeArray<EBiomeName> texture;
        public PathBuffer buffer;
        public ByteArray16 tileCosts;
        public NativeList<int2> outputPath;

        public void Execute()
        {
            outputPath.Clear();
            
            int2 gridSizeXY = new (textureSizeX, textureSizeX);
            
            int endNodeIndex = _createOrGetPathNode(endPosition, ref buffer.pathNodeList, ref buffer.positionToIndexMap);
                
            PathNode startNode = buffer.pathNodeList[_createOrGetPathNode(
                startPosition, 
                ref buffer.pathNodeList, 
                ref buffer.positionToIndexMap)];
               
            startNode.gCost = 0;
            startNode.CalculateFCost();
            buffer.pathNodeList[startNode.index] = startNode;

            buffer.openList.Enqueue(startNode.index, startNode.fCost);
            buffer.openSet.Add(startNode.index);

            while (buffer.openList.Count > 0)
            {
                int currentNodeIndex = buffer.openList.Dequeue();
                   
                buffer.openSet.Remove(currentNodeIndex);
                    
                if (currentNodeIndex == endNodeIndex)
                {
                    break;
                }

                buffer.closedList.Add(currentNodeIndex);

                for (int i = 0; i < NeighbourOffsets.COUNT; i++)
                {
                    int2 offset = NeighbourOffsets.Get(i);
                    
                    PathNode currentNode = buffer.pathNodeList[currentNodeIndex];
                    int2 neighbourPosition = currentNode.position + offset;
                        
                    if (!_isPositionInsideGrid(neighbourPosition, gridSizeXY))
                    {
                        continue;
                    }

                    int neighbourNodeIndex = _createOrGetPathNode(
                        neighbourPosition, 
                        ref buffer.pathNodeList,
                        ref buffer.positionToIndexMap);
                    
                    if (buffer.closedList.Contains(neighbourNodeIndex))
                    {
                        continue;
                    }

                    int tentativeGCost = _calculateTentativeGCost(buffer.pathNodeList, currentNodeIndex, neighbourNodeIndex);
                        
                    PathNode neighbourNode = buffer.pathNodeList[neighbourNodeIndex];

                    if (tentativeGCost < neighbourNode.gCost)
                    {
                        neighbourNode.cameFromNodeIndex = currentNodeIndex;
                        neighbourNode.gCost = tentativeGCost;
                        neighbourNode.CalculateFCost();
                        
                        buffer.pathNodeList[neighbourNodeIndex] = neighbourNode;

                        if (!buffer.openSet.Contains(neighbourNodeIndex))
                        {
                            buffer.openList.Enqueue(neighbourNodeIndex, neighbourNode.fCost);
                            buffer.openSet.Add(neighbourNodeIndex);
                        }
                    }
                }
            }

            _calcPath(buffer.pathNodeList, buffer.pathNodeList[endNodeIndex]);
            
            buffer.pathNodeList.Clear();
            buffer.positionToIndexMap.Clear();
            buffer.openList.Clear();
            buffer.closedList.Clear();
            buffer.openSet.Clear(); 
        }

        private int _calculateTentativeGCost(
            NativeList<PathNode> pathNodeList, 
            int currentNodeIndex,
            int neighbourNodeIndex)
        {
            PathNode neighbourNode = pathNodeList[neighbourNodeIndex];
            int textureIndex = neighbourNode.position.y * textureSizeX + neighbourNode.position.x;
            int tileCost = tileCosts.GetByte((byte)texture[textureIndex]);
            tileCost <<= 1;

            PathNode currentNode = pathNodeList[currentNodeIndex];
            int distanceCost = _calcDistanceCost(currentNode.position, neighbourNode.position);
            int movementCost = BMath.Max(1, tileCost) * distanceCost;
              
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
                position = position,
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
            //return (int)BMath.DistanceSq(start, end);

            int xDistance = BMath.Abs(start.x - end.x);
            int yDistance = BMath.Abs(start.y - end.y);
            int remaining = BMath.Abs(xDistance - yDistance);
            
            return MOVE_DIAGONAL_COST * BMath.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }

        private void _calcPath(NativeList<PathNode> pathNodes, PathNode endNode)
        {
            if (endNode.cameFromNodeIndex == -1)
            {
                return;
            }

            while (endNode.cameFromNodeIndex != -1)
            {
                outputPath.Add(endNode.position);
                endNode = pathNodes[endNode.cameFromNodeIndex];
            }

            outputPath.Add(endNode.position);
        }
    }
}