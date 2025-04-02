using Beast.Radar.Graphic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    [BurstCompile]
    public struct RoadSplineRasterizationJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<int> inCurves;
        [NativeDisableParallelForRestriction, ReadOnly] public RoadSplineMap inRoadMap;
        [ReadOnly] public float2 inCurveOffset;
        [ReadOnly] public int2 inTextureSize;
        [ReadOnly] public int inHalfPerPixel;

        [ReadOnly] public NativeArray<int> inOffsets;
        [ReadOnly] public NativeArray<int> inBufferLimits;

        public NativeArray<int> outPixelCount;
        [NativeDisableParallelForRestriction] public NativeArray<ushort> outPixelHalfs;
        
        public void Execute(int jobIndex)
        {
            int splineIndex = inCurves[jobIndex];

            int2 point0 = BWorld.WorldToTileIndex(inRoadMap.roadPoints[splineIndex] - inCurveOffset);
            int2 point1 = BWorld.WorldToTileIndex(inRoadMap.roadPoints[splineIndex + 1] - inCurveOffset);
            int2 handle = BWorld.WorldToTileIndex(inRoadMap.roadHandles[splineIndex] - inCurveOffset);

            BresenhamRasterizer<TexturePixelPlotterForHalfStream> rasterizer = new()
            {
                plotter = new TexturePixelPlotterForHalfStream
                {
                    writtenPixelCount = 0,
                    textureSize = inTextureSize,
                    halfPerPixel = inHalfPerPixel,
                    output = outPixelHalfs.Slice(inOffsets[jobIndex], inBufferLimits[jobIndex]),
                }
            };

            rasterizer.AddCubicBezier(point0.x, point0.y, handle.x, handle.y, handle.x, handle.y, point1.x, point1.y);

            outPixelCount[jobIndex] = rasterizer.plotter.writtenPixelCount;
        }
    }
}