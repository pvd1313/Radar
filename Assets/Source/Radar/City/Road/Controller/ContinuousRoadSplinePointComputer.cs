using Beast.Claw.Math;
using Unity.Collections;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    /// <summary>
    /// All road splines assumed to be connected. So points evently distribute along road.
    /// </summary>
    public static class ContinuousRoadSplinePointComputer
    {
        public static int ComputeSplinePointCount(float roadLength, float segmentLength)
        {
            return BMath.Max(2, BMath.FloorToInt(roadLength / segmentLength) + 1);
        }

        public static void BuildSpline(ContinuousRoadSplineTask task)
        {
            RoadSplineMap roadMap = task.inRoadMap;
            RoadSpline road = roadMap.roads[task.inRoadIndex];
            float segmentLength = task.inSegmentLength;

            int maxCurvePointCount = 0;
            for (int i = 0; i < road.pointCount - 1; i++)
            {
                float curveLength = roadMap.curveLengths[road.firstPointIndex + i];

                int curvePointCount = BMath.CeilToInt(curveLength / segmentLength);

                maxCurvePointCount = BMath.Max(curvePointCount, maxCurvePointCount);
            }

            maxCurvePointCount++;

            Allocator allocator = JobsUtility.IsExecutingJob ? Allocator.Temp : Allocator.TempJob;
            NativeArray<float> lengthBuffer = new(maxCurvePointCount * RoadUtil.ROAD_LENGTH_TABLE_SCALE, allocator);
            
            float nextPointDistance = segmentLength;

            int maxPointCount = task.inRoadPointCount;

            task.outPoints[0] = roadMap.roadPoints[road.firstPointIndex];
            int writtenPointCount = 1;
            
            for (int splineIndex = 0; splineIndex < road.pointCount - 1 && writtenPointCount < maxPointCount; splineIndex++)
            {
                float curveLength = roadMap.curveLengths[road.firstPointIndex + splineIndex];

                if (nextPointDistance > curveLength)
                {
                    nextPointDistance -= curveLength;
                    continue;
                }

                float splineOffset = nextPointDistance;
                int splinePointCount = BMath.FloorToInt((curveLength - splineOffset) / segmentLength) + 1;
                float splineRemainingDistance = curveLength - (splinePointCount - 1) * segmentLength - splineOffset;
                nextPointDistance = segmentLength - splineRemainingDistance;
                
                BezierCurve2D curve = new ()
                {
                    point0 = roadMap.roadPoints[road.firstPointIndex + splineIndex],
                    point1 = roadMap.roadPoints[road.firstPointIndex + splineIndex + 1],
                    handle0 = roadMap.roadHandles[road.firstPointIndex + splineIndex],
                    handle1 = roadMap.roadHandles[road.firstPointIndex + splineIndex]
                };

                BezierCurveLengthTable lengthTable = BezierCurveLengthTable.Create(
                    curve,
                    lengthBuffer,
                    splinePointCount * RoadUtil.ROAD_LENGTH_TABLE_SCALE,
                    task.precisionMode);

                for (int i = 0; i < splinePointCount && writtenPointCount < maxPointCount; i++)
                {
                    float pointTime = lengthTable.FindTimeForLength(splineOffset + i * segmentLength);

                    if (task.precisionMode == EPrecisionMode.Float)
                    {
                        task.outPoints[writtenPointCount] = curve.ComputePointFloat(pointTime);
                    }
                    else
                    {
                        task.outPoints[writtenPointCount] = (float2)curve.ComputePointDouble(pointTime);
                    }

                    writtenPointCount++;
                }
            }

            task.outPoints[maxPointCount - 1] = roadMap.roadPoints[road.firstPointIndex + road.pointCount - 1];

            lengthBuffer.Dispose();
        }
    }
}