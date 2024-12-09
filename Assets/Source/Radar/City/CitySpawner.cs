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
using UnityEngine;
using UnityEngine.Profiling;
using Random = Unity.Mathematics.Random;
using TerrainData = Beast.Radar.Terrain.TerrainData;

namespace Beast.Radar.City
{
    public class CitySpawner : Actor, ICitySpawner
    {
        public static readonly FloatMinMax MAJOR_CITY_SIZE = new (4, 5);
        public static readonly FloatMinMax MINOR_CITY_SIZE = new (0, 3);
        
        public NativeList<CityInfo> Cities => _cities;
        
        [Required] private readonly IGizmoManager _gizmoManager;
        [Required] private readonly ITerrainBackedBiome _biome;
        [Required] private readonly IWorldManager _worldManager;
        [Required] private readonly CitySpawnConfig _config;

        private IGizmoHandle _gizmo;
        private NativeList<CityInfo> _cities;

        private TerrainData _bakedForTerrain;

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

            float majorGridSize = terrain.terrainSize / _config.majorCityGridScale;
            int majorCityCountX = BMath.CeilToInt(terrain.terrainSize / majorGridSize);
            
            float minorGridSize = terrain.terrainSize / _config.minorCityGridScale;
            int minorCityCountX = BMath.CeilToInt(terrain.terrainSize / minorGridSize);

            int maxCityCount = majorCityCountX * majorCityCountX + minorCityCountX * minorCityCountX;
            _cities = BNative.PermList<CityInfo>(maxCityCount);

            using NativeHashSet<short2> majorCities = 
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
                citySizeRange = MAJOR_CITY_SIZE,
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
                citySizeRange = MINOR_CITY_SIZE,
                useCityPositionCorrection = false,
                excludePositions = majorCities
            };
            
            Profiler.BeginSample("minorCitySpawnJob");
            minorCitySpawnJob.Schedule().Complete();
            Profiler.EndSample();

            RoadSpawnJob roadJob = new()
            {
                inCities = _cities,
                inTerrain1KM = _biome.Terrain1kmCell,
                inTerrain8KM = _biome.Terrain8kmCell,
                outRoadCells8KM = BNative.TempList<int2>(1024)
            };
            _config.roadBuildCost.ToByteArray(out roadJob.inCellBiomeCost, out roadJob.inCellFeatureCost);
            
            Profiler.BeginSample("RoadJob");
            roadJob.Schedule().Complete();
            Profiler.EndSample();

            _drawCityGizmo(roadJob.outRoadCells8KM);

            roadJob.outRoadCells8KM.Dispose();
            
            return UniTask.CompletedTask;
        }

        [Conditional("DEVELOPMENT")]
        private void _drawCityGizmo(NativeList<int2> roadCells8KM)
        {
            int minorCount = 0;
            int majorCount = 0;
            
            _gizmo.Clear();

            int cellSize8KM = _biome.Terrain8kmCell.cellSize;
            
            _gizmo.SetColor(Color.black);

            foreach (int2 position in roadCells8KM)
            {
                _gizmo.AddQuad((float2)position * cellSize8KM + cellSize8KM * 0.5f, cellSize8KM);
            }
            
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
                    _gizmo.SetColor(Color.red);
                    _gizmo.AddPoint(position, 2f * (size + 2));
                }
            }


            BDebug.Track(this, "RoadSegment", roadCells8KM.Length);
            BDebug.Track(this, "TotalCity", _cities.Length);
            BDebug.Track(this, "MajorCity", majorCount);
            BDebug.Track(this, "MinorCity", minorCount);
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