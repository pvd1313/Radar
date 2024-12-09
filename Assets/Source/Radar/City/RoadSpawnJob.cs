using System.Runtime.CompilerServices;
using Beast.Claw.Math;
using Beast.Claw.Math.CCL;
using Beast.Radar.Navigation;
using Beast.Radar.Terrain;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

//using Profiler = UnityEngine.Profiling.Profiler;

namespace Beast.Radar.City
{
    [BurstCompile]
    public struct RoadSpawnJob : IJob
    {
        public NativeList<CityInfo> inCities;
        public TextureWithBlocks<TerrainCell> inTerrain1KM;
        public AveragePixelCountTexture<TerrainCell> inTerrain8KM;
        public ByteArray16 inCellBiomeCost;
        public ByteArray16 inCellFeatureCost;
        
        public NativeList<int2> outRoadCells8KM;

        public void Execute()
        {
            int textureSizeX = inTerrain8KM.cellCountX;

            CCLTextureJob<BiomeCCLTexture> cclJob = new()
            {
                inTexture = new BiomeCCLTexture(inTerrain8KM.texture, textureSizeX),
                outLabeledTexture = new NativeArray<ushort>(textureSizeX * textureSizeX, Allocator.Temp)
            };
            
            // Profiler.BeginSample("cclJob.Execute()");
            cclJob.Execute();
            // Profiler.EndSample();
            
            using NativeTexture<ushort> labels8KM = new (cclJob.outLabeledTexture, textureSizeX);
            using NativeMultiHashMap<ushort, CityInfo> cityGroups = _groupCitiesByLabel(labels8KM);
            using PathJobBuffer pathJobBuffer = PathJobBuffer.Create(1024, Allocator.Temp);
            using NativeArray<ushort> labels = cityGroups.GetKeyArray(Allocator.Temp);
            using NativeList<CityInfo> cityGroup = new (inCities.Length, Allocator.Temp);

            ushort prevLabel = ushort.MaxValue;

            foreach (ushort label in labels)
            {
                if (prevLabel == label)
                {
                    continue;
                }

                prevLabel = label;
                
                cityGroup.Clear();
                
                // Profiler.BeginSample("cityGroups.GetValuesForKey(label)");
                foreach (CityInfo city in cityGroups.GetValuesForKey(label))
                {
                    cityGroup.Add(city);
                }
                // Profiler.EndSample();
                
                // Profiler.BeginSample("_getMajorCitiesAndBounds");
                using NativeList<CityInfo> majorCities = _getMajorCitiesAndBounds(cityGroup, out Bounds2D majorCityBounds);
                // Profiler.EndSample();

                if (majorCities.Length < 2)
                {
                    continue;
                }

                // Profiler.BeginSample("_selectHubCity");
                CityInfo hubCity = _selectHubCity(majorCities, majorCityBounds);
                // Profiler.EndSample();

                // Profiler.BeginSample("_createRoadQueue");
                using NativePriorityQueue<short2> roadQueue = _createRoadQueue(hubCity, majorCities);
                // Profiler.EndSample();

                _buildRoads(hubCity, roadQueue, pathJobBuffer);
            }
        }

        private void _buildRoads(CityInfo hubCity, NativePriorityQueue<short2> queue, PathJobBuffer pathBuffer)
        {
            while (queue.Count > 0)
            {
                FindPathJob<AverageBiomeTexture> job = new()
                {
                    searchBounds = new Bounds2D(0, inTerrain8KM.cellCountX - 1),
                    startPosition = _cityToTexture8KMPosition(queue.Dequeue()),
                    endPosition = _cityToTexture8KMPosition(hubCity.position),
                    buffer = pathBuffer,
                    cellBiomeCost = inCellBiomeCost,
                    cellFeatureCost = inCellFeatureCost,
                    texture = new AverageBiomeTexture(inTerrain8KM)
                };
                
                // Profiler.BeginSample("FindPathJob<AverageBiomeTexture>.Execute()");
                job.Execute();
                // Profiler.EndSample();

                // Profiler.BeginSample("for pathBuffer.outputPathReversed");
                for (int i = pathBuffer.outputPathReversed.Length - 1; i >= 0; i--)
                {
                    int2 position = pathBuffer.outputPathReversed[i];

                    int cellIndex = inTerrain8KM.cellCountX * position.y + position.x;

                    TerrainCell cell = inTerrain8KM.texture[cellIndex];

                    if (cell.HasFeature(ETerrainFeature.Road))
                    {
                        break;
                    }

                    outRoadCells8KM.Add(position);

                    inTerrain8KM.texture[cellIndex] = cell.AddFeature(ETerrainFeature.Road);
                }
                // Profiler.EndSample();
            }
        }
        
        private NativePriorityQueue<short2> _createRoadQueue(CityInfo hubCity, NativeList<CityInfo> majorCities)
        {
            NativePriorityQueue<short2> roadQueue = new(majorCities.Length, Allocator.Temp);

            foreach (CityInfo city in majorCities)
            {
                if (city.position.Equals(hubCity.position))
                {
                    continue;
                }

                float distance = BMath.Distance(city.position, hubCity.position);

                roadQueue.Enqueue(city.position, 8192 - distance);
            }

            return roadQueue;
        }

        private NativeList<CityInfo> _getMajorCitiesAndBounds(NativeList<CityInfo> cities, out Bounds2D bounds)
        {
            bounds = default;
            
            NativeList<CityInfo> major = new(128, Allocator.Temp);
            
            foreach (CityInfo city in cities)
            {
                if (CitySpawner.MAJOR_CITY_SIZE.Contains(city.size))
                {
                    major.Add(city);

                    bounds = bounds.EnsureContains(city.position);
                }
            }

            return major;
        }

        private CityInfo _selectHubCity(NativeList<CityInfo> majorCities, Bounds2D majorCityBounds)
        {
            using NativeList<CityInfo> borderCities = new(16, Allocator.Temp); 
            
            foreach (CityInfo city in majorCities)
            {
                if (majorCityBounds.IsOnBorder(city.position))
                {
                    borderCities.Add(city);
                }
            }
            
            float maxDistance = 0;
            CityInfo hub = default;
                
            for (int i = 0; i < borderCities.Length; i++)
            for (int j = i + 1; j < borderCities.Length; j++)
            {
                CityInfo a = borderCities[i];
                CityInfo b = borderCities[j];
                float distance = BMath.Distance(a.position, b.position);

                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    hub = borderCities[i];
                }
            }

            return hub;
        }
        
        private NativeMultiHashMap<ushort, CityInfo> _groupCitiesByLabel(NativeTexture<ushort> labels8KM)
        {
            NativeMultiHashMap<ushort, CityInfo> cityGroups = new(1024, Allocator.Temp);
            
            foreach (CityInfo cityInfo in inCities)
            {
                int2 pos = _cityToTexture8KMPosition(cityInfo.position);
                ushort label = labels8KM.GetPixel(pos.x, pos.y);

                cityGroups.Add(label, cityInfo);
            }

            return cityGroups;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int2 _cityToTexture8KMPosition(short2 cityPosition)
        {
            return (int2)cityPosition / inTerrain8KM.cellSize;
        }
    }
}