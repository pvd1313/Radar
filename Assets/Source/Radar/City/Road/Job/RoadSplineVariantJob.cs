using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Beast.Radar.City
{
    [BurstCompile]
    public struct RoadSplineVariantJob : IJob
    {
        public RoadSplineMap inSplineMap;
        
        public NativeHashSet<long> outSplineMasks;
        
        public void Execute()
        {
            RoadSplineRasterizer.CollectSplineVariants(inSplineMap, outSplineMasks);
        }
    }
}