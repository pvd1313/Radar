using System.Runtime.CompilerServices;
using Beast.Claw.Math;
using Beast.Radar.Terrain;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Beast.Radar.Navigation
{
    [BurstCompile]
    public struct FindPathJob<TBiomeTexture> : IJob 
        where TBiomeTexture : IBiomeTexture     
    {
        public int2 startPosition;
        public int2 endPosition;
        public TBiomeTexture texture;
        public Bounds2D searchBounds;
        public PathJobBuffer buffer;
        public ByteArray16 cellBiomeCost;
        public ByteArray16 cellFeatureCost;

        public void Execute()
        {
            buffer.Clear();
            
            int endNodeIndex = _createOrGetPathNode(
                endPosition, 
                ref buffer.pathNodeList, 
                ref buffer.positionToIndexMap);
                
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

                for (int i = 0; i < NeighbourOffset.COUNT; i++)
                {
                    int2 offset = NeighbourOffset.Get(i);
                    
                    PathNode currentNode = buffer.pathNodeList[currentNodeIndex];
                    int2 neighbourPosition = currentNode.position + offset;

                    if (!searchBounds.Contains(neighbourPosition))
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

                    float tentativeGCost = _calculateTentativeGCost(buffer.pathNodeList, currentNodeIndex, neighbourNodeIndex);
                        
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

            _buildPath(buffer.pathNodeList, buffer.pathNodeList[endNodeIndex]);
        }

        private float _calculateTentativeGCost(
            NativeList<PathNode> pathNodeList, 
            int currentNodeIndex,
            int siblingNodeIndex)
        {
            PathNode siblingNode = pathNodeList[siblingNodeIndex];
            TerrainCell siblingCell = texture.GetPixel(siblingNode.position);
            int siblingCost = cellBiomeCost.GetByte((byte)siblingCell.Biome);
            siblingCost = BMath.Min(siblingCost, siblingCell.GetFeatureMinCost(cellFeatureCost));
            
            PathNode currentNode = pathNodeList[currentNodeIndex];
            TerrainCell currentCell = texture.GetPixel(currentNode.position);
            int currentCost = cellBiomeCost.GetByte((byte)currentCell.Biome);
            currentCost = BMath.Min(currentCost, currentCell.GetFeatureMinCost(cellBiomeCost));
            
            float distanceCost = _calcDistanceCost(currentNode.position, siblingNode.position);
            float movementCost = BMath.Max(1, siblingCost) * distanceCost * 0.5f;
            movementCost += BMath.Max(1, currentCost) * distanceCost * 0.5f;
              
            return currentNode.gCost + movementCost;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float _calcDistanceCost(int2 start, int2 end)
        {
            return BMath.Distance(start, end);
        }

        private void _buildPath(NativeList<PathNode> pathNodes, PathNode endNode)
        {
            if (endNode.cameFromNodeIndex == -1)
            {
                return;
            }

            while (endNode.cameFromNodeIndex != -1)
            {
                buffer.outputPathReversed.Add(endNode.position);
                endNode = pathNodes[endNode.cameFromNodeIndex];
            }

            buffer.outputPathReversed.Add(endNode.position);
        }
    }
}