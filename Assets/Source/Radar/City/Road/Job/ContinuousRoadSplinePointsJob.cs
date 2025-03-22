using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    [BurstCompile]
    public struct ContinuousRoadSplinePointsJob : IJobParallelFor
    {
        [ReadOnly] public RoadSplineMap inRoadMap;
        [ReadOnly] public EPrecisionMode inPrecisionMode;
        [ReadOnly] public float inSegmentLength;
        [ReadOnly] public NativeArray<int> inPointCount;
        [ReadOnly] public NativeArray<int> inPointsOffset;

        [NativeDisableParallelForRestriction] public NativeArray<float2> outPoints;
        
        public void Execute(int roadIndex)
        {
            int roadPointCount = inPointCount[roadIndex];

            ContinuousRoadSplinePointComputer.BuildSpline(new ContinuousRoadSplineTask
            {
                inRoadIndex = roadIndex,
                inRoadMap = inRoadMap,
                inSegmentLength = inSegmentLength,
                outPoints = outPoints.Slice(inPointsOffset[roadIndex], roadPointCount),
                precisionMode = inPrecisionMode,
                inRoadPointCount = roadPointCount
            });
        }
    }
}