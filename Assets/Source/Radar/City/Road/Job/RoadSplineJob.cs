using System.Runtime.CompilerServices;
using Beast.Claw.Math;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    /// <summary>
    /// Converts road paths to bezier splines.
    /// </summary>
    [BurstCompile]
    public struct RoadSplineJob : IJobParallelFor
    {
        [ReadOnly, NativeDisableParallelForRestriction] public NativeArray<ushort2> inRoadPathPoints;
        [ReadOnly] public NativeArray<RoadPath> inRoadPaths;
        [ReadOnly] public int inMaxPointCountPerRoad;

        [NativeDisableParallelForRestriction] public NativeArray<float2> outRoadSplinePoints;
        [NativeDisableParallelForRestriction] public NativeArray<float2> outRoadSplineHandles;
        public NativeArray<int> outRoadSplinePointCount;

        private RoadPath _roadPath;
        
        public void Execute(int roadIndex)
        {
            _roadPath = inRoadPaths[roadIndex];

            int splinePointCount = 0;

            int pointIndex = 0;

            int writeOffset = inMaxPointCountPerRoad * roadIndex;
            
            while (splinePointCount < inMaxPointCountPerRoad)
            {
                if (pointIndex == _roadPath.pointCount - 1)
                {
                    outRoadSplinePoints[writeOffset + splinePointCount] = _getRoadPoint(pointIndex) + new float2(0.5f);

                    splinePointCount++;
                    
                    break;
                }
                
                int endPointIndex = _skipPointsUntilStraightLineBreak(pointIndex);

                float2 lineStart = _getRoadPoint(pointIndex) + new float2(0.5f);
                float2 lineEnd = _getRoadPoint(endPointIndex) + new float2(0.5f);;
                
                if (pointIndex == 0)
                {
                    if (endPointIndex == _roadPath.pointCount - 1)
                    {
                        outRoadSplinePoints[writeOffset + 0] = lineStart;
                        outRoadSplinePoints[writeOffset + 1] = lineEnd;
                        outRoadSplineHandles[writeOffset] = (lineStart + lineEnd) * 0.5f;
                        
                        splinePointCount = 2;
                        
                        break;
                    }

                    outRoadSplinePoints[writeOffset] = lineStart;
                    outRoadSplineHandles[writeOffset] = lineEnd;

                    splinePointCount++;
                    pointIndex = endPointIndex;
                    
                    continue;
                }

                outRoadSplinePoints[writeOffset + splinePointCount] = (lineStart + lineEnd) * 0.5f;
                outRoadSplineHandles[writeOffset + splinePointCount] = lineEnd;

                splinePointCount++;
                pointIndex = endPointIndex;
            }

            outRoadSplinePointCount[roadIndex] = splinePointCount;
        }

        private int _skipPointsUntilStraightLineBreak(int pointIndex)
        {
            int2 point0 = _getRoadPoint(pointIndex);

            pointIndex++;
            
            int2 point1 = _getRoadPoint(pointIndex);

            int2 motionDelta = point1 - point0;
            
            while (pointIndex < _roadPath.pointCount - 1)
            {
                pointIndex++;

                (point0, point1) = (point1, _getRoadPoint(pointIndex));

                if (!(point1 - point0).Equals(motionDelta))
                {
                    pointIndex--;
                    break;
                }
            }

            return pointIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ushort2 _getRoadPoint(int pointIndex)
        {
            return inRoadPathPoints[_roadPath.startPointIndex + pointIndex];
        }
    }
}