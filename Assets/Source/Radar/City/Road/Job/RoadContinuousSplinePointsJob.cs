using Beast.Claw.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    [BurstCompile]
    public struct RoadContinuousSplinePointsJob : IMultiJob
    {
        [ReadOnly] public RoadSplineMap inRoadMap;
        [ReadOnly] public EPrecisionMode inPrecision;
        [ReadOnly] public float inSegmentLength;
        [ReadOnly] public NativeArray<int> inPointCount;
        [ReadOnly] public NativeArray<int> inPointsOffset;

        [NativeDisableParallelForRestriction] public NativeArray<float2> outPoints;
        
        public void Execute(int roadIndex)
        {
            int roadPointCount = inPointCount[roadIndex];

            RoadContinuousSplineUtil.Build(new RoadContinuousSplineTask
            {
                inRoadIndex = roadIndex,
                inRoadMap = inRoadMap,
                inSegmentLength = inSegmentLength,
                outPoints = outPoints.Slice(inPointsOffset[roadIndex], roadPointCount),
                inPrecision = inPrecision,
                inRoadPointCount = roadPointCount
            });
        }
    }
}