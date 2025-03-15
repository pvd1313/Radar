using System.Diagnostics;
using Beast.Claw.Attributes;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Claw.Gizmo;
using Beast.Claw.Jobs;
using Beast.Claw.Math;
using Beast.Radar.Terrain;
using Beast.Radar.World;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Profiling;

namespace Beast.Radar.City
{
    public class CitySpawner : Actor, ICitySpawner
    {
        public static readonly FloatMinMax MAJOR_CITY_SIZE = new (4, 5);
        public static readonly FloatMinMax MINOR_CITY_SIZE = new (0, 3);
        
        public NativeSlice<CityInfo> Cities => _cities.AsArray().Slice();
        
        [Required] private readonly IGizmoManager _gizmoManager;
        [Required] private readonly ITerrainBackedBiome _biome;
        [Required] private readonly IWorldManager _worldManager;
        [Required] private readonly CitySpawnConfig _config;

        private NativeList<CityInfo> _cities;
        
        private TerrainData _bakedForTerrain;

        protected override UniTask _warmup(ConcurrentContext context)
        {
            _cities = BNative.PermList<CityInfo>(1024);
            
            return UniTask.CompletedTask;
        }

        protected override UniTask _gameLoad(ConcurrentContext context)
        {
            TerrainData terrain = _worldManager.GetCurrentWorld().terrain;

            if (terrain.IsSame(_bakedForTerrain))
            {
                return UniTask.CompletedTask;
            }

            _bakedForTerrain = terrain;

            float majorGridSize = _config.majorCityGridSize;
            int majorCityCountX = BMath.CeilToInt(terrain.terrainSize / majorGridSize);
            
            float minorGridSize = _config.minorCityGridSize;
            int minorCityCountX = BMath.CeilToInt(terrain.terrainSize / minorGridSize);

            _cities.Clear();

            using NativeHashSet<ushort2> majorCities = 
                BNative.TempJobHashSet<ushort2>(majorCityCountX * majorCityCountX);

            CitySpawnJob majorCitySpawnJob = new ()
            {
                gridSize = majorGridSize,
                collisionRadius = majorGridSize * 0.5f,
                random = new Random(BMath.Max(1, (uint)terrain.seed)),
                maxCityCountX = majorCityCountX,
                mapBounds = new Bounds2D(0, terrain.terrainSize - 1),
                outputCities = _cities,
                terrain1KM = _biome.Terrain1kmCell,
                terrain8KM = _biome.Terrain8kmCell,
                citySizeRange = MAJOR_CITY_SIZE,
                useCityPositionCorrection = true,
                excludePositions = majorCities
            };
            
            Profiler.BeginSample("majorCitySpawnJob");
            majorCitySpawnJob.Run();
            Profiler.EndSample();
            
            for (int i = 0; i < _cities.Length; i++)
            {
                majorCities.Add(_cities[i].position);
            }
            
            CitySpawnJob minorCitySpawnJob = new()
            {
                gridSize = minorGridSize,
                collisionRadius = minorGridSize * 0.3f,
                random = new Random(BMath.Max(1, (uint)terrain.seed + 1)),
                maxCityCountX = minorCityCountX,
                mapBounds = new Bounds2D(0, terrain.terrainSize - 1),
                outputCities = _cities,
                terrain1KM = _biome.Terrain1kmCell,
                terrain8KM = _biome.Terrain8kmCell,
                citySizeRange = MINOR_CITY_SIZE,
                useCityPositionCorrection = false,
                excludePositions = majorCities
            };
            
            Profiler.BeginSample("minorCitySpawnJob");
            minorCitySpawnJob.Run();
            Profiler.EndSample();

            _debugTrack();
            
            return UniTask.CompletedTask;
        }

        [Conditional("DEVELOPMENT")]
        private void _debugTrack()
        {
            int minorCount = 0;
            int majorCount = 0;
        
            foreach (CityInfo city in _cities)
            {
                if (city.IsMajorCity())
                {
                    majorCount++;
                }
                else
                {
                    minorCount++;
                }
            }

            BDebug.Track(this, "Total", _cities.Length);
            BDebug.Track(this, "Major", majorCount);
            BDebug.Track(this, "Minor", minorCount);
        }

        protected override void _dispose(BlockingContext context)
        {
            _cities.Dispose();
        }
    }
}