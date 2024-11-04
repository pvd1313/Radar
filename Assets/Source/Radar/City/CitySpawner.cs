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

        private GizmosHandle _normalCityGizmo;
        private GizmosHandle _warningCityGizmo;
        private GizmosHandle _brokenCityGizmo;
        private NativeList<CityInfo> _cities;

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
            _normalCityGizmo = _gizmosManager.GetHandle(this, "normal", Color.cyan);
            _brokenCityGizmo = _gizmosManager.GetHandle(this, "broken", Color.magenta);
            _warningCityGizmo = _gizmosManager.GetHandle(this, "warning", Color.yellow);
            
            return UniTask.CompletedTask;
        }

        private UniTask _gameLoad(ConcurrentContext context)
        {
            _dispose(default);

            TerrainData terrain = _worldManager.GetCurrentWorld().terrain; 

            float gridSize = terrain.terrainSize / 8.0f;
            int maxCityCount = BMath.CeilToInt(terrain.terrainSize / gridSize);

            CitySpawnJob job = new ()
            {
                gridSize = gridSize,
                collisionRadius = gridSize * 0.5f,
                random = new Random(BMath.Max(1, (uint)terrain.seed)),
                maxCityCountX = maxCityCount,
                mapBounds = new FloatBounds2(0, terrain.terrainSize - 1),
                outputCities = NativeMemory.CreatePermList<CityInfo>(maxCityCount * maxCityCount),
                terrain1km = _biome.Terrain1kmCell,
                terrain8km = _biome.Terrain8kmCell
            };
            
            Profiler.BeginSample("CitySpawnJob.Execute()");
            job.Schedule().Complete();
            Profiler.EndSample();

            _cities = job.outputCities;
            
            return UniTask.CompletedTask;
        }

        private void _render(BlockingContext context)
        {
            if (!_cities.IsCreated)
            {
                return;
            }
            
            foreach (CityInfo cityInfo in _cities)
            {
                switch (cityInfo.status)
                {
                    case ECityStatus.Failed:
                        _brokenCityGizmo.DrawScreenDot(cityInfo.originalPosition);
                        break;
                    
                    case ECityStatus.Placed:
                        _normalCityGizmo.DrawScreenDot(cityInfo.originalPosition);
                        break;
                    
                    case ECityStatus.PlacedButCorrected:
                        _normalCityGizmo.DrawScreenDot(cityInfo.correctedPosition);

                        _warningCityGizmo.DrawLine(cityInfo.originalPosition, cityInfo.correctedPosition);

                        for (float t = 0; t < 1; t += 0.2f)
                        {
                            float2 pos = BMath.Lerp(cityInfo.originalPosition, cityInfo.correctedPosition, t);
                            
                            _warningCityGizmo.DrawScreenDot(pos);
                        }
                        break;
                }
            }

            BDebug.Track(this, "CityCount", _cities.Length);
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