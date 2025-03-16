using Beast.Claw.Math;
using Beast.Radar.Navigation;
using Beast.Radar.Terrain;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    [BurstCompile]
    public struct Road1KMJob : IJobParallelFor
    {
        [ReadOnly] public NativeSlice<RoadSegment> inSegments;
        [ReadOnly] public TextureWithBlocks<TerrainCell> inTerrainCells1KM;
        [ReadOnly] public int inMaxPointPerSegment;
        [ReadOnly] public ByteArray16 inCellBiomeCost;
        [ReadOnly] public ByteArray16 inCellFeatureCost;

        [NativeDisableParallelForRestriction] public NativeArray<ushort2> outPoints;
        [NativeDisableParallelForRestriction] public NativeArray<RoadSegment> outSegments;
        public NativeArray<int> outSegmentPointCount;
        
        public void Execute(int segmentIndex)
        {
            PathJobBuffer pathBuffer = PathJobBuffer.Create(4096, Allocator.TempJob);
            RoadSegment segment = inSegments[segmentIndex];

            FindPathJob<BiomeTextureWithBlocks> pathJob = new()
            {
                startPosition = segment.point0,
                endPosition = segment.point1,
                texture = new BiomeTextureWithBlocks(inTerrainCells1KM),
                buffer = pathBuffer,
                searchBounds = segment.bounds,
                cellBiomeCost = inCellBiomeCost,
                cellFeatureCost = inCellFeatureCost
            };
            
            pathJob.Execute();

            RoadSegment newSegment = default;
            newSegment.point0 = segment.point0;
            newSegment.bounds = Bounds2D.FromPoint(segment.point0);

            int pointCount = (ushort)BMath.Min(inMaxPointPerSegment, pathJob.buffer.outputPathReversed.Length);

            int writeOffset = segmentIndex * inMaxPointPerSegment;

            for (int i = 0; i < pointCount; i++)
            {
                int2 point = pathJob.buffer.outputPathReversed[^(i + 1)];

                newSegment.bounds = newSegment.bounds.EnsureContains(point);
                
                outPoints[writeOffset + i] = (ushort2)point;
            }

            newSegment.point1 = (ushort2)pathJob.buffer.outputPathReversed[^pointCount];

            outSegments[segmentIndex] = newSegment;
            outSegmentPointCount[segmentIndex] = pointCount;

            pathBuffer.Dispose();
        }
    }
}