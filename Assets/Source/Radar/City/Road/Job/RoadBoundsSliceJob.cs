using Beast.Claw.Collections;
using Beast.Claw.Jobs;
using Beast.Claw.Math;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Beast.Radar.City
{
    [BurstCompile]
    public struct RoadBoundsSliceJob : IJob
    {
        public Bounds2D inBounds;
        public CellGrid<int> inRoadGrid;
        public RoadSplineMap inRoadMap;
        
        public NativeArray<RoadSplineSlice> outRoadSlices;
        public NativeReference<int> outRoadSliceCount;
        public NativeReference<int> outRoadCurveCount;

        public void Execute()
        {
            NativeArray<int> roadsBuffer = BNative.TempArray<int>(16);
            NativeSlice<int> roads = inRoadGrid.GetItems(inBounds, roadsBuffer);

            int totalCurveCount = 0;
            int totalSliceCount = 0;
            
            for (int i = 0; i < roads.Length; i++)
            {
                RoadSlicedSplineUtil.CutAABB(
                    inRoadMap, 
                    roads[i], 
                    inBounds,
                    outRoadSlices.Slice(totalSliceCount), 
                    out int sliceCount);
                
                for (int j = 0; j < sliceCount; j++)
                {
                    RoadSplineSlice slice = outRoadSlices[totalSliceCount + j];

                    totalCurveCount += slice.pointCount - 1;
                }
                
                totalSliceCount += sliceCount;
            }

            outRoadCurveCount.Value = totalCurveCount;
            outRoadSliceCount.Value = totalSliceCount;

            roadsBuffer.Dispose();
        }
    }
}