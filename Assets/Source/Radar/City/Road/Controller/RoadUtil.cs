using Beast.Claw.Attributes.Annotation;
using Beast.Claw.Jobs;
using Beast.Claw.Math;
using Beast.Radar.Navigation;
using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    [StaticUtility]
    public static class RoadUtil
    {
        public static RoadPathMap CreateRoadPathMap(RoadPath1KMJob job, int maxPointPerRoad, Allocator allocator)
        {
            int totalPointCount = 0;
            foreach (int pointCount in job.outSegmentPointCount)
            {
                totalPointCount += pointCount;
            }

            NativeArray<ushort2> roadPoints = new(totalPointCount, allocator);
            NativeArray<RoadPath> roads = new (job.outSegments.Length, allocator);

            int writeOffset = 0;
            int readOffset = 0;
            for (int i = 0; i < job.outSegments.Length; i++)
            {
                RoadSegment segment = job.outSegments[i];
                int pointCount = job.outSegmentPointCount[i];

                BNative.Copy(job.outPoints, roadPoints, readOffset, writeOffset, pointCount);

                roads[i] = new RoadPath
                {
                    startPointIndex = (ushort)writeOffset,
                    pointCount = (ushort)pointCount,
                    bounds = segment.bounds,
                };

                writeOffset += pointCount;
                readOffset += maxPointPerRoad;
            }

            return new RoadPathMap
            {
                roads = roads,
                roadPoints = roadPoints
            };
        }

        public static RoadSplineMap CreateRoadSplineMap(
            RoadPathMap pathMap,
            RoadSplineJob job, 
            int maxPointPerRoad, 
            Allocator allocator)
        {
            int totalPointCount = 0;
            foreach (int pointCount in job.outRoadSplinePointCount)
            {
                totalPointCount += pointCount;
            }

            NativeArray<float2> roadPoints = new(totalPointCount, allocator);
            NativeArray<float2> roadHandles = new(totalPointCount, allocator);
            NativeArray<RoadSpline> roads = new(pathMap.roads.Length, allocator);

            int writeOffset = 0;
            int readOffset = 0;
            for (int i = 0; i < job.outRoadSplinePointCount.Length; i++)
            {
                int pointCount = job.outRoadSplinePointCount[i];

                BNative.Copy(job.outRoadSplinePoints, roadPoints, readOffset, writeOffset, pointCount);
                BNative.Copy(job.outRoadSplineHandles, roadHandles, readOffset, writeOffset, pointCount);

                roads[i] = new RoadSpline
                {
                    startPointIndex = (ushort)writeOffset,
                    pointCount = (ushort)pointCount,
                    bounds = pathMap.roads[i].bounds
                };
                
                writeOffset += pointCount;
                readOffset += maxPointPerRoad;
            }

            return new RoadSplineMap
            {
                roadPoints = roadPoints,
                roadHandles = roadHandles,
                roads = roads,
            };
        }
        
        public static NativeList<RoadSegment> CreateRoadSegments(
            RoadPath8KMJob job, 
            int terrainSize, 
            int maxSegmentSize,
            Allocator allocator)
        {
            NativeList<RoadSegment> resultSegments = new (256, allocator);

            using NativeList<float2> km8Points = BNative.TempJobList<float2>(16);

            int pointOffset = 0;
            
            for (int segID = 0; segID < job.outSegments.Length; segID++)
            {
                int segPointCount = job.outSegments[segID];
                
                int pointIndex0 = pointOffset;
                ushort2 startPoint8KM = job.outPoints[pointIndex0];
                ushort2 junction0 = job.outJunctions[startPoint8KM];
                
                km8Points.Clear();
                km8Points.Add(job.inOutTerrain8KM.GetCellCenterWorld(startPoint8KM));
                
                int pointIndex1 = pointIndex0 + 1;
                
                pointOffset += segPointCount;

                while (pointIndex0 < pointOffset - 1)
                {
                    int pointCount = pointIndex1 - pointIndex0;

                    ushort2 endPoint8K = job.outPoints[pointIndex1];
                    km8Points.Add(job.inOutTerrain8KM.GetCellCenterWorld(endPoint8K));
                    
                    bool endIsJunction = job.outJunctions.TryGetValue(endPoint8K, out ushort2 junction1);

                    if (pointCount >= BMath.Min(maxSegmentSize, segPointCount) || endIsJunction)
                    {
                        if (!endIsJunction)
                        {
                            junction1 = SelectJunctionForCell(endPoint8K, job.inOutTerrain8KM.cellSize);
                        }

                        Bounds2D searchBounds = BNavigation.CreateSearchBounds(new NavigationPathSearchBounds
                        {
                            startCell = junction0,
                            finishCell = junction1,
                            cell8KMSize = job.inOutTerrain8KM.cellSize,
                            include8KMWorldPoints = km8Points.AsArray(),
                            terrainSize = terrainSize
                        });
                        
                        resultSegments.Add(new RoadSegment
                        {
                            point0 = junction0,
                            point1 = junction1,
                            bounds = searchBounds
                        });
                        
                        pointIndex0 = pointIndex1;
                        junction0 = junction1;
                    }
                    
                    pointIndex1++;
                }
            }

            return resultSegments;
        }
        
        public static ushort2 SelectJunctionForCell(int2 cellIndex, int cellSize)
        {
            int2 junction = cellIndex * cellSize;
            
            junction += cellSize / 2 - 1;

            return (ushort2)junction;
        }
    }
}