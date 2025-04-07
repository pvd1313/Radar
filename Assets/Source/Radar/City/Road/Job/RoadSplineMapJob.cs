using System.Runtime.CompilerServices;
using Beast.Claw.Jobs;
using Beast.Claw.Math;
using Beast.Radar.Graphic;
using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    /// <summary>
    /// Converts road paths to bezier splines.
    /// </summary>
    public struct RoadSplineMapJob : IMultiJob
    {
        [ReadOnly, NativeDisableParallelForRestriction] public NativeArray<ushort2> inRoadPathPoints;
        [ReadOnly] public NativeArray<RoadPath> inRoadPaths;
        [ReadOnly] public int inMaxPointCountPerRoad;

        [NativeDisableParallelForRestriction] public NativeArray<float2> outRoadSplinePoints;
        [NativeDisableParallelForRestriction] public NativeArray<float2> outRoadSplineHandles;
        [NativeDisableParallelForRestriction] public NativeArray<float> outRoadCurveLengths;
        public NativeArray<int> outRoadSplinePointCount;
        public NativeArray<float> outRoadLength;

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
                    float2 endPoint = _roadPointToWorld(_getRoadPoint(pointIndex));
                    outRoadSplinePoints[writeOffset + splinePointCount - 1] = endPoint;
                    
                    break;
                }
                
                int endPointIndex = _getLineEndPoint(pointIndex);

                float2 lineStart = _roadPointToWorld(_getRoadPoint(pointIndex));
                float2 lineEnd = _roadPointToWorld(_getRoadPoint(endPointIndex));
                
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
                }
                else
                {
                    outRoadSplinePoints[writeOffset + splinePointCount] = (lineStart + lineEnd) * 0.5f;
                    outRoadSplineHandles[writeOffset + splinePointCount] = lineEnd;
                }

                splinePointCount++;
                pointIndex = endPointIndex;
            }

            outRoadSplinePointCount[roadIndex] = splinePointCount;

            float roadLength = 0;
            for (int i = 0; i < splinePointCount - 1; i++)
            {
                BezierCurve2D curve = new()
                {
                    point0 = outRoadSplinePoints[writeOffset + i],
                    point1 = outRoadSplinePoints[writeOffset + i + 1],
                    handle0 = outRoadSplineHandles[writeOffset + i],
                    handle1 = outRoadSplineHandles[writeOffset + i]
                };

                float curveLength = curve.ComputeLength();

                outRoadCurveLengths[writeOffset + i] = curveLength;
                
                roadLength += curveLength;
            }

            outRoadLength[roadIndex] = roadLength;
        }

        private int _getLineEndPoint(int pointIndex)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float2 _roadPointToWorld(int2 roadPoint)
        {
            const float TILE_OFFSET = 0.5f - 0.5f / BWorld.TILES_PER_CELL;
            
            return roadPoint + new float2(TILE_OFFSET);
        }
    }
}