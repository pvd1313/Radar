using Beast.Claw.Math;
using Unity.Collections;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    public static class RoadSlicedSplineUtil
    {
        public static void CutAABB(
            RoadSplineMap roadMap, 
            int roadIndex, 
            Bounds2D viewBounds,
            NativeSlice<RoadSplineSlice> buffer,
            out int sliceCount)
        {
            sliceCount = 0;

            RoadSpline road = roadMap.roads[roadIndex];

            int endPointIndex = 0;

            while (endPointIndex < road.pointCount - 1 && sliceCount < buffer.Length)
            {
                int sliceStart = endPointIndex;
                
                float2 point0 = roadMap.roadPoints[road.firstPointIndex + endPointIndex];
                float2 point1 = roadMap.roadPoints[road.firstPointIndex + endPointIndex + 1];

                // Find slice start.
                Bounds2D curveBounds = Bounds2D.FromTwoPoints(point0, point1);
                if (!viewBounds.Intersects(curveBounds))
                {
                    endPointIndex++;
                    continue;
                }

                endPointIndex++;

                // Find slice end.
                while (endPointIndex < road.pointCount - 1)
                {
                    point0 = roadMap.roadPoints[road.firstPointIndex + endPointIndex];
                    point1 = roadMap.roadPoints[road.firstPointIndex + endPointIndex + 1];

                    curveBounds = Bounds2D.FromTwoPoints(point0, point1);

                    if (!viewBounds.Intersects(curveBounds))
                    {
                        break;
                    }

                    endPointIndex++;
                }

                buffer[sliceCount] = new RoadSplineSlice
                {
                    roadIndex = roadIndex,
                    pointCount = endPointIndex - sliceStart + 1,
                    firstPointIndex = sliceStart
                };

                sliceCount++;
            }
        }

        public static int GetPointCount(RoadSplineMap roadMap, float segmentLength, RoadSplineSlice slice)
        {
            int pointCount = 0;
            
            RoadSpline road = roadMap.roads[slice.roadIndex];
                
            pointCount += slice.pointCount;
                
            for (int i = 0; i < slice.pointCount - 1; i++)
            {
                float curveLength = roadMap.curveLengths[road.firstPointIndex + slice.firstPointIndex + i];
                int curvePointCount = BMath.FloorToInt(curveLength / segmentLength);

                pointCount += curvePointCount;
            }

            return pointCount;
        }

        public static void Build(RoadSlicedSplineTask task, out int writtenPointCount)
        {
            RoadSplineMap roadMap = task.inRoadMap;
            float segmentLength = task.inSegmentLength;
            RoadSplineSlice slice = task.inRoadSlice;
            RoadSpline road = roadMap.roads[slice.roadIndex];

            int maxCurvePointCount = 0;
                
            for (int i = 0; i < slice.pointCount - 1; i++)
            {
                float curveLength = roadMap.curveLengths[road.firstPointIndex + slice.firstPointIndex + i];

                int curvePointCount = BMath.FloorToInt(curveLength / segmentLength);

                maxCurvePointCount = BMath.Max(curvePointCount, maxCurvePointCount);
            }

            maxCurvePointCount++;
            
            Allocator allocator = JobsUtility.IsExecutingJob ? Allocator.Temp : Allocator.TempJob;
            NativeArray<float> lengthBuffer = new(maxCurvePointCount * RoadUtil.ROAD_LENGTH_TABLE_SCALE, allocator);

            int maxPointCount = task.inRoadPointCount;
            writtenPointCount = 0;

            task.outPoints[writtenPointCount] = roadMap.roadPoints[road.firstPointIndex + slice.firstPointIndex];
                
            writtenPointCount++;

            for (int i = 0; i < slice.pointCount - 1 && writtenPointCount < maxPointCount; i++)
            {
                int firstPointIndex = road.firstPointIndex + slice.firstPointIndex + i;
                    
                float curveLength = roadMap.curveLengths[firstPointIndex];
                    
                int curvePointCount = BMath.FloorToInt(curveLength / segmentLength);

                if (curvePointCount > 0)
                {
                    BezierCurve2D curve = new()
                    {
                        point0 = roadMap.roadPoints[firstPointIndex],
                        point1 = roadMap.roadPoints[firstPointIndex + 1],
                        handle0 = roadMap.roadHandles[firstPointIndex],
                        handle1 = roadMap.roadHandles[firstPointIndex]
                    };
                        
                    BezierCurveLengthTable lengthTable = BezierCurveLengthTable.Create(
                        curve,
                        lengthBuffer,
                        curvePointCount * RoadUtil.ROAD_LENGTH_TABLE_SCALE,
                        task.inPrecision);

                    float curveDelta = curveLength / (curvePointCount + 1);
                        
                    for (int j = 1; j <= curvePointCount && writtenPointCount < maxPointCount; j++)
                    {
                        float pointTime = lengthTable.FindTimeForLength(j * curveDelta);

                        if (task.inPrecision == EPrecisionMode.Float)
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

                if (writtenPointCount < maxPointCount)
                {
                    task.outPoints[writtenPointCount] = roadMap.roadPoints[firstPointIndex + 1];

                    writtenPointCount++;
                }
            }

            lengthBuffer.Dispose();
        }
    }
}