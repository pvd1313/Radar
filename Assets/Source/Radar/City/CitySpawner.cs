using System.Diagnostics;
using Beast.Claw.Common;
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
using UnityEngine;
using UnityEngine.Profiling;

using Random = Unity.Mathematics.Random;
using TerrainData = Beast.Radar.Terrain.TerrainData;

namespace Beast.Radar.City
{
    public class CitySpawner : Actor, ICitySpawner
    {
        public NativeList<CityInfo> Cities => _cities;
        
        private readonly IGizmoManager _gizmoManager;
        private readonly ITerrainBackedBiome _biome;
        private readonly IWorldManager _worldManager;

        private IGizmoHandle _gizmo;
        private NativeList<CityInfo> _cities;

        private TerrainData _bakedForTerrain;

        public CitySpawner(
            IGizmoManager gizmoManager, 
            ITerrainBackedBiome biome, 
            IWorldManager worldManager)
        {
            _gizmoManager = gizmoManager;
            _biome = biome;
            _worldManager = worldManager;
        }

        protected override UniTask _warmup(ConcurrentContext context)
        {
            _gizmo = _gizmoManager.CreateHandle(this);
            
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

            if (_cities.IsCreated)
            {
                _cities.Dispose();
            }

            float majorGridSize = terrain.terrainSize / 8.0f;
            int majorCityCountX = BMath.CeilToInt(terrain.terrainSize / majorGridSize);
            
            float minorGridSize = terrain.terrainSize / 32.0f;
            int minorCityCountX = BMath.CeilToInt(terrain.terrainSize / minorGridSize);

            int maxCityCount = majorCityCountX * majorCityCountX + minorCityCountX * minorCityCountX;
            _cities = BNative.PermList<CityInfo>(maxCityCount);

            NativeHashSet<short2> majorCities =
                BNative.TempHashSet<short2>(majorCityCountX * majorCityCountX);

            CitySpawnJob majorCitySpawnJob = new ()
            {
                gridSize = majorGridSize,
                collisionRadius = majorGridSize * 0.5f,
                random = new Random(BMath.Max(1, (uint)terrain.seed)),
                maxCityCountX = majorCityCountX,
                mapBounds = new Bounds2D(0, terrain.terrainSize - 1),
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
                mapBounds = new Bounds2D(0, terrain.terrainSize - 1),
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

            _drawCityGizmo();
            
            return UniTask.CompletedTask;
        }

        [Conditional("DEVELOPMENT")]
        private void _drawCityGizmo()
        {
            int minorCount = 0;
            int majorCount = 0;
            
            _gizmo.Clear();
            
            foreach (CityInfo cityInfo in _cities)
            {
                int size = cityInfo.size;
                
                float2 position = cityInfo.position + new float2(0.5f);

                if (size >= 4)
                {
                    majorCount++;
                    _gizmo.SetColor(Color.cyan);
                    _gizmo.AddPoint(position, 3 * size);
                }
                else
                {
                    minorCount++;
                    _gizmo.SetColor(Color.gray);
                    _gizmo.AddPoint(position, 2f * (size + 2));
                }
            }

            BDebug.Track(this, "Total", _cities.Length);
            BDebug.Track(this, "Major", majorCount);
            BDebug.Track(this, "Minor", minorCount);
        }

        protected override void _dispose(BlockingContext context)
        {
            if (_cities.IsCreated)
            {
                _cities.Dispose();
            }
            
            _gizmo.Dispose();
        }

    }
}