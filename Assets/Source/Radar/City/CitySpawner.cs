using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Gizmos;
using Beast.Claw.Jobs;
using Beast.Claw.Mathematics;
using Beast.Radar.Terrain;
using Beast.Radar.World;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

using Random = Unity.Mathematics.Random;
using TerrainData = Beast.Radar.Terrain.TerrainData;

namespace Beast.Radar.City
{
    public class CitySpawner : Controller, IGameCycleListener
    {
        private readonly IGizmosManager _gizmosManager;
        private readonly ITerrainBackedBiome _biome;
        private readonly IWorldManager _worldManager;

        private GizmosHandle _majorCityGizmo;
        private GizmosHandle _minorCityGizmos;
        private NativeList<CityInfo> _cities;

        private TerrainData _bakedForTerrain;

        public CitySpawner(
            IGizmosManager gizmosManager, 
            ITerrainBackedBiome biome, 
            IWorldManager worldManager)
        {
            _gizmosManager = gizmosManager;
            _biome = biome;
            _worldManager = worldManager;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.GAME_LOAD, _gameLoad);
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
            handle.ListenBlocking(ActionID.DISPOSE, _dispose);
            handle.ListenBlocking(ActionID.RENDER, _render);
        }

        private UniTask _warmup(ConcurrentContext context)
        {
            _majorCityGizmo = _gizmosManager.GetHandle(this, "normal", Color.cyan);
            _minorCityGizmos = _gizmosManager.GetHandle(this, "broken", Color.white);
            
            return UniTask.CompletedTask;
        }

        private UniTask _gameLoad(ConcurrentContext context)
        {
            TerrainData terrain = _worldManager.GetCurrentWorld().terrain;

            if (terrain.IsSame(_bakedForTerrain))
            {
                return UniTask.CompletedTask;
            }

            _bakedForTerrain = terrain;
            
            _dispose(default);

            float majorGridSize = terrain.terrainSize / 8.0f;
            int majorCityCountX = BMath.CeilToInt(terrain.terrainSize / majorGridSize);
            
            float minorGridSize = terrain.terrainSize / 32.0f;
            int minorCityCountX = BMath.CeilToInt(terrain.terrainSize / minorGridSize);

            int maxCityCount = majorCityCountX * majorCityCountX + minorCityCountX * minorCityCountX;
            _cities = Native.CreatePermList<CityInfo>(maxCityCount);

            NativeHashSet<short2> majorCities =
                Native.CreateTempHashSet<short2>(majorCityCountX * majorCityCountX);

            CitySpawnJob majorCitySpawnJob = new ()
            {
                gridSize = majorGridSize,
                collisionRadius = majorGridSize * 0.5f,
                random = new Random(BMath.Max(1, (uint)terrain.seed)),
                maxCityCountX = majorCityCountX,
                mapBounds = new FloatBounds2(0, terrain.terrainSize - 1),
                outputCities = _cities,
                terrain1km = _biome.Terrain1kmCell,
                terrain8km = _biome.Terrain8kmCell,
                citySizeRange = new FloatMinMax(4, 5),
                useCityPositionCorrection = true,
                excludePositions = majorCities
            };
            
            Profiler.BeginSample("majorCitySpawnJob");
            majorCitySpawnJob.Schedule().Complete();
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
                mapBounds = new FloatBounds2(0, terrain.terrainSize - 1),
                outputCities = _cities,
                terrain1km = _biome.Terrain1kmCell,
                terrain8km = _biome.Terrain8kmCell,
                citySizeRange = new FloatMinMax(0, 3),
                useCityPositionCorrection = false,
                excludePositions = majorCities
            };
            
            Profiler.BeginSample("minorCitySpawnJob");
            minorCitySpawnJob.Schedule().Complete();
            Profiler.EndSample();

            majorCities.Dispose();
            
            return UniTask.CompletedTask;
        }

        private void _render(BlockingContext context)
        {
            if (!_cities.IsCreated)
            {
                return;
            }

            int majorCount = 0;
            int minorCount = 0;
            
            foreach (CityInfo cityInfo in _cities)
            {
                int size = cityInfo.size;
                
                float2 position = cityInfo.position + new float2(0.5f);

                if (size >= 4)
                {
                    majorCount++;
                    _majorCityGizmo.DrawPoint(position, 3 * size);
                }
                else
                {
                    minorCount++;
                    _minorCityGizmos.DrawPoint(position, 2f * (size + 2));
                }
            }

            BDebug.Track(this, "Total", _cities.Length);
            BDebug.Track(this, "Major", majorCount);
            BDebug.Track(this, "Minor", minorCount);
        }
        
        private void _dispose(BlockingContext obj)
        {
            if (_cities.IsCreated)
            {
                _cities.Dispose();
            }
        }
    }
}