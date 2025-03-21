using Beast.Claw.Jobs;
using Beast.Claw.Math;
using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    public static class RoadSplineBuilder
    {
        public static int GetSplinePointCount(RoadSplineTask task)
        {
            RoadSpline road = task.inRoadMap.roads[task.inRoadIndex];

            return BMath.FloorToInt(road.length / task.inSegmentLength) + 2;
        }

        public static void BuildSpline(RoadSplineTask task)
        {
            const int LENGTH_TABLE_MUL = 3;

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
            
            NativeArray<float> lengthBuffer = BNative.TempArray<float>(maxCurvePointCount * LENGTH_TABLE_MUL);
            float nextPointDistance = segmentLength;

            int maxPointCount = GetSplinePointCount(task);

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
                    splinePointCount * LENGTH_TABLE_MUL,
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