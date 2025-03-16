// #define ROAD_PROFILING

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Beast.Claw.Math;
using Beast.Claw.Math.CCL;
using Beast.Radar.Navigation;
using Beast.Radar.Terrain;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    [Unity.Burst.BurstCompile]
    public struct Road8KMJob : IJob
    {
        public NativeSlice<CityInfo> inCities;
        public AveragePixelCountTexture<TerrainCell> inOutTerrain8KM;
        public ByteArray16 inCellBiomeCost;
        public ByteArray16 inCellFeatureCost;
        public ERoadSpawnPriority inSpawnPriority;
        
        public NativeList<ushort2> outPoints;
        public NativeHashMap<ushort2, ushort2> outJunctions;
        public NativeList<ushort> outSegments;

        public void Execute()
        {
            int textureSizeX = inOutTerrain8KM.cellCountX;

            CCLTextureJob<BiomeCCLTexture> cclJob = new()
            {
                inTexture = new BiomeCCLTexture(inOutTerrain8KM.texture, textureSizeX),
                outLabeledTexture = new NativeArray<ushort>(textureSizeX * textureSizeX, Allocator.Temp)
            };
            
            _beginSample("cclJob.Execute()");
            cclJob.Execute();
            _endSample();
            
            using NativeTexture<ushort> labels8KM = new (cclJob.outLabeledTexture, textureSizeX);
            using NativeParallelMultiHashMap<ushort, CityInfo> cityGroups = _groupCitiesByLabel(labels8KM);
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
                
                _beginSample("cityGroups.GetValuesForKey(label)");
                foreach (CityInfo city in cityGroups.GetValuesForKey(label))
                {
                    cityGroup.Add(city);
                }
                _endSample();
                
                _beginSample("_getMajorCitiesAndBounds");
                using NativeList<CityInfo> majorCities = _getMajorCities(cityGroup);
                _endSample();

                if (majorCities.Length < 2)
                {
                    continue;
                }

                _beginSample("_selectHubCity");
                CityInfo hubCity = _selectCityOnBorder(majorCities);
                _endSample();

                _beginSample("_createRoadQueue");
                using NativePriorityQueue<ushort2> roadQueue = _createRoadQueue(hubCity, majorCities, inSpawnPriority);
                _endSample();

                _buildRoads(hubCity, roadQueue, pathJobBuffer);
            }
        }

        private void _buildRoads(CityInfo hubCity, NativePriorityQueue<ushort2> queue, PathJobBuffer pathBuffer)
        {
            int2 hubCityPos8K = _cityToTexture8KMPosition(hubCity.position);
            outJunctions[(ushort2)hubCityPos8K] = hubCity.position;
            
            while (queue.Count > 0)
            {
                ushort2 currentCityPos = queue.Dequeue();
                int2 currentCityPos8K = _cityToTexture8KMPosition(currentCityPos);
                outJunctions[(ushort2)currentCityPos8K] = currentCityPos;
                
                FindPathJob<TerrainMotionCostComputer> pathJob = new()
                {
                    searchBounds = new Bounds2D(0, inOutTerrain8KM.cellCountX - 1),
                    startPosition = currentCityPos8K,
                    endPosition = hubCityPos8K,
                    buffer = pathBuffer,
                    costComputer = new TerrainMotionCostComputer
                    {
                        cellFeatureCost = inCellFeatureCost,
                        cellBiomeCost = inCellBiomeCost,
                        terrain8KMTexture = inOutTerrain8KM,
                        use8KMTerrainTexture = true
                    }
                };
                
                _beginSample("FindPathJob<AverageBiomeTexture>.Execute()");
                pathJob.Execute();
                _endSample();

                int pointCount = 0;

                _beginSample("for pathBuffer.outputPathReversed");
                for (int i = pathBuffer.outputPathReversed.Length - 1; i >= 0; i--)
                {
                    int2 position = pathBuffer.outputPathReversed[i];
                    int cellIndex = inOutTerrain8KM.cellCountX * position.y + position.x;
                    TerrainCell cell = inOutTerrain8KM.texture[cellIndex];

                    pointCount++;

                    if (cell.HasFeature(ETerrainFeature.Road))
                    {
                        break;
                    }

                    inOutTerrain8KM.texture[cellIndex] = cell.AddFeature(ETerrainFeature.Road);
                }
                _endSample();

                if (pointCount < 2)
                {
                    continue;
                }

                outSegments.Add((ushort)pointCount);

                for (int i = 0; i < pointCount; i++)
                {
                    int2 position = pathBuffer.outputPathReversed[^ (i + 1)];

                    outPoints.Add((ushort2)position);
                }

                ushort2 lastPoint = outPoints[^1];

                if (!outJunctions.TryGetValue(lastPoint, out _))
                {
                    outJunctions[lastPoint] = RoadUtil.SelectJunctionForCell(lastPoint, inOutTerrain8KM.cellSize);
                }
            }
        }

        private Bounds2D _getCityBounds(NativeList<CityInfo> cities)
        {
            Bounds2D bounds = Bounds2D.FromPoint(cities[0].position);
            
            for (int i = 1; i < cities.Length; i++)
            {
                bounds = bounds.EnsureContains(cities[i].position);
            }

            return bounds;
        }
        
        private NativePriorityQueue<ushort2> _createRoadQueue(
            CityInfo hubCity, 
            NativeList<CityInfo> otherCities, 
            ERoadSpawnPriority priorityMethod)
        {
            if (priorityMethod == ERoadSpawnPriority.None)
            {
                throw new ArgumentException("priorityMethod");
            }
            
            NativePriorityQueue<ushort2> roadQueue = new(otherCities.Length, Allocator.Temp);

            foreach (CityInfo city in otherCities)
            {
                if (city.position.Equals(hubCity.position))
                {
                    continue;
                }

                float distance = BMath.Distance(city.position, hubCity.position);

                if (priorityMethod == ERoadSpawnPriority.FirstAreFarthest)
                {
                    distance = 8192 - distance;
                }

                roadQueue.Enqueue(city.position, distance);
            }

            return roadQueue;
        }

        private NativeList<CityInfo> _getMajorCities(NativeList<CityInfo> cities)
        {
            NativeList<CityInfo> major = new(128, Allocator.Temp);
            
            foreach (CityInfo city in cities)
            {
                if (city.IsMajor())
                {
                    major.Add(city);
                }
            }

            return major;
        }

        private CityInfo _selectCityOnBorder(NativeList<CityInfo> cities)
        {
            _beginSample("_getCityBounds");
            Bounds2D cityBounds = _getCityBounds(cities);
            _endSample();
            
            using NativeList<CityInfo> borderCities = new(16, Allocator.Temp); 
            
            foreach (CityInfo city in cities)
            {
                if (cityBounds.IsOnBorder(city.position))
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
        
        private NativeParallelMultiHashMap<ushort, CityInfo> _groupCitiesByLabel(NativeTexture<ushort> labels8KM)
        {
            NativeParallelMultiHashMap<ushort, CityInfo> cityGroups = new(1024, Allocator.Temp);
            
            foreach (CityInfo cityInfo in inCities)
            {
                int2 pos = _cityToTexture8KMPosition(cityInfo.position);
                ushort label = labels8KM.GetPixel(pos.x, pos.y);

                cityGroups.Add(label, cityInfo);
            }

            return cityGroups;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int2 _cityToTexture8KMPosition(ushort2 cityPosition)
        {
            return (int2)cityPosition / inOutTerrain8KM.cellSize;
        }

        [Conditional("ROAD_PROFILING")]
        private static void _beginSample(string name)
        {
            UnityEngine.Profiling.Profiler.BeginSample(name);
        }

        [Conditional("ROAD_PROFILING")]
        private static void _endSample()
        {
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }
}