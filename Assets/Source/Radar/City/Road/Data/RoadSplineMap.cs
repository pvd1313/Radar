using System;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.Jobs;
using Beast.Claw.Math;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    public struct RoadSplineMap : IDisposable
    {
        public NativeArray<RoadSpline> roads;
        public NativeArray<float2> roadPoints;
        public NativeArray<float2> roadHandles;
        public NativeArray<float> curveLengths;
        
        public static RoadSplineMap Create(RoadPathMap roadPathMap, Allocator allocator)
        {
            int maxRoadPointCount = 0;
            for (int i = 0; i < roadPathMap.roads.Length; i++)
            {
                maxRoadPointCount = BMath.Max(maxRoadPointCount, roadPathMap.roads[i].pointCount);
            }
            
            int roadCount = roadPathMap.roads.Length;

            RoadSplineMapJob splineMapJob = new()
            {
                inRoadPaths = roadPathMap.roads,
                inRoadPathPoints = roadPathMap.roadPoints,
                inMaxPointCountPerRoad = maxRoadPointCount,
                outRoadSplinePoints = BNative.TempJobArray<float2>(maxRoadPointCount * roadCount),
                outRoadSplineHandles = BNative.TempJobArray<float2>(maxRoadPointCount * roadCount),
                outRoadCurveLengths = BNative.TempJobArray<float>(maxRoadPointCount * roadCount),
                outRoadSplinePointCount = BNative.TempJobArray<int>(roadCount),
                outRoadLength = BNative.TempJobArray<float>(roadCount),
            };
            
            BProfiler.BeginSample("RoadSplineJob");
            splineMapJob.Schedule(roadCount, 8).Complete();
            BProfiler.EndSample();

            int totalPointCount = 0;
            foreach (int pointCount in splineMapJob.outRoadSplinePointCount)
            {
                totalPointCount += pointCount;
            }

            NativeArray<float2> roadPoints = new(totalPointCount, allocator);
            NativeArray<float2> roadHandles = new(totalPointCount, allocator);
            NativeArray<float> curveLengths = new(totalPointCount, allocator);
            NativeArray<RoadSpline> roads = new(roadPathMap.roads.Length, allocator);

            int writeOffset = 0;
            int readOffset = 0;
            for (int i = 0; i < splineMapJob.outRoadSplinePointCount.Length; i++)
            {
                int pointCount = splineMapJob.outRoadSplinePointCount[i];

                BNative.Copy(splineMapJob.outRoadSplinePoints, roadPoints, readOffset, writeOffset, pointCount);
                BNative.Copy(splineMapJob.outRoadSplineHandles, roadHandles, readOffset, writeOffset, pointCount);
                BNative.Copy(splineMapJob.outRoadCurveLengths, curveLengths, readOffset, writeOffset, pointCount);

                roads[i] = new RoadSpline
                {
                    firstPointIndex = (ushort)writeOffset,
                    pointCount = (ushort)pointCount,
                    bounds = roadPathMap.roads[i].bounds,
                    length = splineMapJob.outRoadLength[i]
                };
                
                writeOffset += pointCount;
                readOffset += maxRoadPointCount;
            }
            
            splineMapJob.outRoadSplineHandles.Dispose();
            splineMapJob.outRoadSplinePoints.Dispose();
            splineMapJob.outRoadCurveLengths.Dispose();
            splineMapJob.outRoadSplinePointCount.Dispose();
            splineMapJob.outRoadLength.Dispose();

            return new RoadSplineMap
            {
                roadPoints = roadPoints,
                roadHandles = roadHandles,
                roads = roads,
                curveLengths = curveLengths
            };
        }
        
        public void Dispose()
        {
            roads.DisposeIfCreated();
            roadPoints.DisposeIfCreated();
            roadHandles.DisposeIfCreated();
            curveLengths.DisposeIfCreated();
        }
    }
}