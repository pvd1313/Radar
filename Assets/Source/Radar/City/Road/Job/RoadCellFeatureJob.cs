using Beast.Claw.Jobs;
using Beast.Claw.Math;
using Beast.Radar.Terrain;
using Unity.Burst;
using Unity.Collections;

namespace Beast.Radar.City
{
    /// <summary>
    /// Add ETerrainFeature.Road to cells containing roads.
    /// </summary>
    [BurstCompile]
    public struct RoadCellFeatureJob : IMultiJob
    {
        [ReadOnly] public RoadPathMap inRoadMap;

        [NativeDisableParallelForRestriction] public AveragePixelCountTexture<TerrainCell> inOutTerrain8KM;
        [NativeDisableParallelForRestriction] public TextureWithBlocks<TerrainCell> inOutTerrain1KM;
        
        public void Execute(int roadIndex)
        {
            RoadPath roadPath = inRoadMap.roads[roadIndex];

            for (int i = 0; i < roadPath.pointCount; i++)
            {
                ushort2 roadPoint = inRoadMap.roadPoints[roadPath.startPointIndex + i];

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