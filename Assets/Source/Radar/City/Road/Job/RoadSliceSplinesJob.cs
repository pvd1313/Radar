using Beast.Claw.Collections;
using Beast.Claw.Jobs;
using Beast.Claw.Math;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Beast.Radar.City
{
    [BurstCompile]
    public struct RoadSliceSplinesJob : IJobParallelFor
    {
        [ReadOnly, NativeDisableParallelForRestriction] public RoadSplineMap inRoadMap;
        [ReadOnly, NativeDisableParallelForRestriction] public CellGrid<int> inRoadGrid;
        [ReadOnly] public NativeArray<Bounds2D> inBounds;
        [ReadOnly] public float inSegmentLength;
        [ReadOnly] public int inMaxSliceCount;

        public NativeArray<int> outSliceCount;
        [NativeDisableParallelForRestriction] public NativeArray<RoadSplineSlice> outSlices;
        [NativeDisableParallelForRestriction] public NativeArray<int> outSlicePointCount;
        
        public void Execute(int jobIndex)
        {
            NativeArray<int> roadsBuffer = BNative.TempArray<int>(16);

            Bounds2D jobBounds = inBounds[jobIndex];

            NativeSlice<int> roads = inRoadGrid.GetItems(jobBounds, roadsBuffer);

            int totalSliceCount = 0;

            for (int i = 0; i < roads.Length && totalSliceCount < inMaxSliceCount; i++)
            {
                int firstSliceIndex = inMaxSliceCount * jobIndex + totalSliceCount;
                
                RoadSplineSliceUtil.CutAABB(
                    inRoadMap, 
                    roads[i], 
                    jobBounds, 
                    outSlices.Slice(firstSliceIndex, inMaxSliceCount - totalSliceCount),
                    out int sliceCount);

                for (int j = 0; j < sliceCount; j++)
                {
                    outSlicePointCount[firstSliceIndex + j] = RoadSplineSliceUtil.GetPointCount(
                        inRoadMap, 
                        inSegmentLength, 
                        outSlices[firstSliceIndex + j]);
                }

                totalSliceCount += sliceCount;
            }

            outSliceCount[jobIndex] = totalSliceCount;
            
            roadsBuffer.Dispose();
        }
    }
}