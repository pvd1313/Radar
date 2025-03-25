using Beast.Claw.Math;
using Beast.Radar.Navigation;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    [BurstCompile]
    public struct RoadPath1KMJob : IJobParallelFor
    {
        [ReadOnly] public NativeSlice<RoadSegment> inSegments;
        [ReadOnly] public int inMaxPointPerSegment;
        [ReadOnly] public TerrainMotionCostComputer motionCost;

        [NativeDisableParallelForRestriction] public NativeArray<ushort2> outPoints;
        [NativeDisableParallelForRestriction] public NativeArray<RoadSegment> outSegments;
        public NativeArray<int> outSegmentPointCount;
        
        public void Execute(int segmentIndex)
        {
            PathJobBuffer pathBuffer = PathJobBuffer.Create(4096, Allocator.TempJob);
            RoadSegment segment = inSegments[segmentIndex];

            FindPathJob<TerrainMotionCostComputer> pathJob = new()
            {
                startPosition = segment.point0,
                endPosition = segment.point1,
                costComputer = motionCost,
                buffer = pathBuffer,
                searchBounds = segment.bounds,
            };
            
            pathJob.Execute();

            RoadSegment newSegment = default;
            newSegment.point0 = segment.point0;
            newSegment.bounds = Bounds2D.FromPoint(segment.point0 + new float2(0.5f));

            int pointCount = (ushort)BMath.Min(inMaxPointPerSegment, pathJob.buffer.outputPathReversed.Length);

            int writeOffset = segmentIndex * inMaxPointPerSegment;

            for (int i = 0; i < pointCount; i++)
            {
                int2 point = pathJob.buffer.outputPathReversed[^(i + 1)];

                newSegment.bounds = newSegment.bounds.EnsureContains(point + new float2(0.5f));
                
                outPoints[writeOffset + i] = (ushort2)point;
            }

            newSegment.point1 = (ushort2)pathJob.buffer.outputPathReversed[^pointCount];

            outSegments[segmentIndex] = newSegment;
            outSegmentPointCount[segmentIndex] = pointCount;

            pathBuffer.Dispose();
        }
    }
}