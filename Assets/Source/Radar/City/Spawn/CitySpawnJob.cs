using Beast.Claw.Math;
using Beast.Radar.Terrain;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    [BurstCompile]
    public struct CitySpawnJob : IJob
    {
        public int maxCityCountX;
        public float gridSize;
        public float collisionRadius;
        public Random random;
        public Bounds2D mapBounds;
        public AveragePixelCountTexture<TerrainCell> terrain8KM;
        public TextureWithBlocks<TerrainCell> terrain1KM;
        public FloatMinMax citySizeRange;
        public bool useCityPositionCorrection;
        public NativeHashSet<short2> excludePositions;
        
        public NativeList<CityInfo> outputCities;
        
        public void Execute()
        {
            Bounds2D globalBounds = default;
            bool hasExcludePositions = !excludePositions.IsEmpty;

            for (int y = 0; y < maxCityCountX; y++)
            {
                Bounds2D localBounds = default;

                for (int x = 0; x < maxCityCountX; x++)
                {
                    float2 randomFloat2 = random.NextFloat2();
                    float2 estimatedPosition = new float2(x, y) * gridSize + new float2(gridSize * 0.5f);
                    float2 randomXY =  randomFloat2 * 2.0f - 1.0f;
                    estimatedPosition += randomXY * gridSize * 0.5f;
                    
                    int2 finalPosition = BMath.FloorToInt(estimatedPosition);
                    
                    if (localBounds.Contains(estimatedPosition) || globalBounds.Contains(estimatedPosition))
                    {
                        continue;
                    }

                    if (!_tryCorrectCityCenter(ref finalPosition))
                    {
                        continue;
                    }
                    
                    if (localBounds.Contains(finalPosition) || globalBounds.Contains(finalPosition))
                    {
                        continue;
                    }

                    if (hasExcludePositions && excludePositions.Contains((short2)finalPosition))
                    {
                        continue;
                    }

                    localBounds.x.max = BMath.Max(finalPosition.x + collisionRadius, localBounds.x.max);
                    localBounds.y.max = BMath.Max(finalPosition.y + collisionRadius, localBounds.y.max);
                    
                    outputCities.Add(new CityInfo
                    {
                        position = (short2) finalPosition,
                        size = (byte) BMath.Round(BMath.Lerp(citySizeRange, randomFloat2.x))
                    });       
                }
                
                globalBounds.x.max = BMath.Max(localBounds.x.max, globalBounds.x.max);
                globalBounds.y.max = BMath.Max(localBounds.y.max, globalBounds.y.max);
            }
        }

        private bool _tryCorrectCityCenter(ref int2 center)
        {
            if (_isValidCellForCity(terrain1KM.GetPixelClamped(center)))
            {
                if (!useCityPositionCorrection)
                {
                    return true;
                }
                
                // Try to move city closer to the ocean
                for (int radius = 0; radius < 15; radius++)
                {
                    CitySpawnPointSearch iterator = new(radius);

                    while (iterator.GotNext(out int2 delta))
                    {
                        int2 position = center + delta * terrain8KM.cellSize;
                        if (!mapBounds.Contains(position) ||
                            terrain8KM.GetPixel(position).Biome != ETerrainBiome.Shallow ||
                            !_tryFindValidPlaceInCell8km(position, out int2 newPosition))
                        {
                            continue;
                        }

                        center = newPosition;
                        
                        return true;
                    }
                }

                return true;
            }

            if (!useCityPositionCorrection)
            {
                return false;
            }
            
            // Try moving city from deep ocean to the nearest land.
            for (int radius = 0; radius < 24; radius++)
            {
                CitySpawnPointSearch iterator = new(radius);

                while (iterator.GotNext(out int2 delta))
                {
                    int2 position = center + delta * terrain8KM.cellSize;

                    if (!mapBounds.Contains(position) || 
                        terrain8KM.GetPixel(position).Biome == ETerrainBiome.Deep ||
                        !_tryFindValidPlaceInCell8km(position, out int2 newPosition))
                    {
                        continue;
                    }
                    
                    center = newPosition;
                    
                    return true;
                }
            }

            return false;
        }
        
        private bool _tryFindValidPlaceInCell8km(int2 referencePosition, out int2 newPosition)
        {
            int2 cellPosition = terrain8KM.FloorToCellWorldPosition(referencePosition);
            int blockOffset = terrain1KM.GetBlockOffset(cellPosition);
            int cellSize = terrain8KM.cellSize;

            for (int i = 0; i < cellSize * cellSize; i++)
            {
                int2 pixelXY = new (i % cellSize, i / cellSize);

                int innerOffset = terrain1KM.GetInnerOffset(cellPosition + pixelXY);

                TerrainCell cell = terrain1KM.texture[blockOffset + innerOffset];

                if (_isValidCellForCity(cell))
                {
                    newPosition = cellPosition + pixelXY;
                    return true;
                }
            }

            newPosition = default;

            return false;
        }
        
        bool _isValidCellForCity(TerrainCell cell)
        {
            ETerrainBiome biome = cell.Biome;
            
            return biome != ETerrainBiome.Deep &&
                   biome != ETerrainBiome.Shallow;
        }
    }
}