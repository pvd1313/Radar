using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    [BurstCompile]
    public struct RoadSlicedSplinePointsJob : IJobParallelFor
    {
        [ReadOnly, NativeDisableContainerSafetyRestriction] public RoadSplineMap inRoadMap;
        [ReadOnly] public NativeArray<RoadSplineSlice> inRoadSlices;

        [ReadOnly] public float inSegmentLength;
        [ReadOnly] public EPrecisionMode inPrecision;
        [ReadOnly] public NativeArray<int> inPointCount;
        [ReadOnly] public NativeArray<int> inPointOffset;

        [NativeDisableContainerSafetyRestriction] public NativeArray<float2> outPoints;
        public NativeArray<int> outPointCount;
        
        public void Execute(int sliceIndex)
        {
            int pointCount = inPointCount[sliceIndex];

            RoadSplineSliceUtil.Build(new RoadSlicedSplineTask
                {
                    inRoadMap = inRoadMap,
                    inPrecision = inPrecision,
                    inRoadPointCount = pointCount,
                    inRoadSlice = inRoadSlices[sliceIndex],
                    inSegmentLength = inSegmentLength,
                    outPoints = outPoints.Slice(inPointOffset[sliceIndex], pointCount)
                },
                out pointCount);

            outPointCount[sliceIndex] = pointCount;
        }
    }
}