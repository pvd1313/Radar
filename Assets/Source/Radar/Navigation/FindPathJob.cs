using Beast.Claw.Jobs;
using Beast.Claw.Math;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Radar.Navigation
{
    [BurstCompile]
    public struct FindPathJob<TCostComputer> : ISingleJob 
        where TCostComputer : IMotionCostComputer     
    {
        public int2 startPosition;
        public int2 endPosition;
        public TCostComputer costComputer;
        public Bounds2D searchBounds;
        public PathJobBuffer buffer;

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
            PathNode currentNode = pathNodeList[currentNodeIndex];
            PathNode siblingNode = pathNodeList[siblingNodeIndex];
            
            float movementCost = costComputer.ComputeCost(new MotionCostInput
            {
                motionStartPoint = currentNode.position,
                motionEndPoint = siblingNode.position,
                pathStartPoint = startPosition,
                pathEndPoint = endPosition
            });
            
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
                hCost = BMath.Distance(position, endPosition),
                cameFromNodeIndex = -1
            });

            pathNodeList[newIndex].CalculateFCost();
            
            return newIndex;
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