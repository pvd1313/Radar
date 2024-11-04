using Beast.Claw.Mathematics;
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
        public FloatBounds2 mapBounds;
        public AveragePixelCountTexture<EBiomeName> terrain8km;
        public TextureWithBlocks<EBiomeName> terrain1km;
        
        public NativeList<CityInfo> outputCities;
        
        public void Execute()
        {
            FloatBounds2 globalBounds = default;

            for (int y = 0; y < maxCityCountX; y++)
            {
                FloatBounds2 localBounds = default;

                for (int x = 0; x < maxCityCountX; x++)
                {
                    float2 cityCenter = new float2(x, y) * gridSize + new float2(gridSize * 0.5f);
                    float2 randomXY =  random.NextFloat2() * 2.0f - 1.0f;
                    cityCenter += randomXY * gridSize * 0.5f;
                    
                    cityCenter = BMath.FloorToInt(cityCenter) + new float2(0.5f);
                    
                    if (localBounds.Contains(cityCenter) || globalBounds.Contains(cityCenter))
                    {
                        continue;
                    }
                    
                    float2 orgPosition = cityCenter;

                    _correctCityCenter(ref cityCenter, out ECityStatus status);

                    localBounds.x.max = BMath.Max(cityCenter.x + collisionRadius, localBounds.x.max);
                    localBounds.y.max = BMath.Max(cityCenter.y + collisionRadius, localBounds.y.max);
                    
                    outputCities.Add(new CityInfo
                    {
                        originalPosition = orgPosition,
                        correctedPosition = cityCenter,
                        status = status
                    });       
                }
                
                globalBounds.x.max = BMath.Max(localBounds.x.max, globalBounds.x.max);
                globalBounds.y.max = BMath.Max(localBounds.y.max, globalBounds.y.max);
            }
        }

        private void _correctCityCenter(ref float2 center, out ECityStatus status)
        {
            int2 centerInt = BMath.FloorToInt(center);

            if (_isValidBiomeForCity(terrain1km.GetPixelClamped(centerInt)))
            {
                // Try to move city closer to the ocean
                for (int radius = 0; radius < 15; radius++)
                {
                    CitySpawnPointSearch iterator = new(radius);

                    while (iterator.GotNext(out int2 delta))
                    {
                        int2 position = centerInt + delta * terrain8km.cellSize;
                        if (!mapBounds.Contains(position) ||
                            terrain8km.GetPixel(position) != EBiomeName.Shallow ||
                            !_tryFindValidPlaceInCell8km(position, out int2 newPosition))
                        {
                            continue;
                        }

                        center = newPosition + new float2(0.5f);
                        status = ECityStatus.PlacedButCorrected;
                        return;
                    }
                }

                status = ECityStatus.Placed;
                return;
            }
            
            // Try moving city from deep ocean to the nearest land.
            for (int radius = 0; radius < 24; radius++)
            {
                CitySpawnPointSearch iterator = new(radius);

                while (iterator.GotNext(out int2 delta))
                {
                    int2 position = centerInt + delta * terrain8km.cellSize;

                    if (!mapBounds.Contains(position) || 
                        terrain8km.GetPixel(position) == EBiomeName.Deep ||
                        !_tryFindValidPlaceInCell8km(position, out int2 newPosition))
                    {
                        continue;
                    }
                    
                    center = newPosition + new float2(0.5f);
                    status = ECityStatus.PlacedButCorrected;
                    return;
                }
            }
            
            status = ECityStatus.Failed;
        }
        
        private bool _tryFindValidPlaceInCell8km(int2 referencePosition, out int2 newPosition)
        {
            int2 cellPosition = terrain8km.FloorToCellWorldPosition(referencePosition);
            int blockOffset = terrain1km.GetBlockOffset(cellPosition);
            int cellSize = terrain8km.cellSize;

            for (int i = 0; i < cellSize * cellSize; i++)
            {
                int2 pixelXY = new (i % cellSize, i / cellSize);

                int innerOffset = terrain1km.GetInnerOffset(cellPosition + pixelXY);

                EBiomeName biome = terrain1km.texture[blockOffset + innerOffset];

                if (_isValidBiomeForCity(biome))
                {
                    newPosition = cellPosition + pixelXY;
                    return true;
                }
            }

            newPosition = default;

            return false;
        }
        
        bool _isValidBiomeForCity(EBiomeName biomeName)
        {
            return biomeName != EBiomeName.Deep &&
                   biomeName != EBiomeName.Shallow;
        }
    }
}