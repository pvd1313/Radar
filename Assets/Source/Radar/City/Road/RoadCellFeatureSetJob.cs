using Beast.Claw.Math;
using Beast.Radar.Terrain;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Beast.Radar.City
{
    [BurstCompile]
    public struct RoadCellFeatureSetJob : IJobParallelFor
    {
        [ReadOnly] public NativeSlice<RoadInfo> inRoads;
        [ReadOnly] public NativeSlice<ushort2> inRoadPoints;

        [NativeDisableParallelForRestriction] public AveragePixelCountTexture<TerrainCell> inOutTerrain8KM;
        [NativeDisableParallelForRestriction] public TextureWithBlocks<TerrainCell> inOutTerrain1KM;
        
        public void Execute(int roadIndex)
        {
            RoadInfo road = inRoads[roadIndex];

            for (int i = 0; i < road.pointCount; i++)
            {
                ushort2 roadPoint = inRoadPoints[road.startPointIndex + i];

                int pixelOffset = inOutTerrain1KM.GetPixelOffset(roadPoint);
                TerrainCell cell = inOutTerrain1KM.texture[pixelOffset];
                inOutTerrain1KM.texture[pixelOffset] = cell.AddFeature(ETerrainFeature.Road);

                pixelOffset = inOutTerrain8KM.GetPixelOffset1KM(roadPoint);
                cell = inOutTerrain8KM.texture[pixelOffset];
                inOutTerrain8KM.texture[pixelOffset] = cell.AddFeature(ETerrainFeature.Road);
            }
        }
    }
}