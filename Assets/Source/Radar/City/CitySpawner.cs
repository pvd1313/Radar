using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Gizmos;
using Beast.Claw.Mathematics;
using Beast.Radar.Serialization;
using Beast.Radar.Terrain;
using Cysharp.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Beast.Radar.City
{
    public class CitySpawner : Controller, IGameCycleListener
    {
        private enum ECityStatus
        {
            Invalid,
            Placed,
            PlacedButCorrected,
            Failed
        }
        
        private struct CityInfo
        {
            public float2 correctedPosition;
            public float2 originalPosition;
            public ECityStatus status;
        }
        
        private readonly IGizmosManager _gizmosManager;
        private readonly ITerrainBackedBiome _biome;
        private readonly GameModel _game;

        private GizmosHandle _normalCityGizmo;
        private GizmosHandle _warningCityGizmo;
        private List<CityInfo> _cities;
        private GizmosHandle _brokenCityGizmo;

        public CitySpawner(
            IGizmosManager gizmosManager, 
            ITerrainBackedBiome biome, 
            GameModel game)
        {
            _gizmosManager = gizmosManager;
            _biome = biome;
            _game = game;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.GAME_LOAD, _gameLoad);
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
            handle.ListenBlocking(ActionID.RENDER, _render);
        }

        private UniTask _warmup(ConcurrentContext context)
        {
            _normalCityGizmo = _gizmosManager.GetHandle(this, "normal", Color.cyan);
            _brokenCityGizmo = _gizmosManager.GetHandle(this, "broken", Color.magenta);
            _warningCityGizmo = _gizmosManager.GetHandle(this, "warning", Color.yellow);
            _cities = new List<CityInfo>(256);
            
            return UniTask.CompletedTask;
        }

        private UniTask _gameLoad(ConcurrentContext context)
        {
            float CITY_GRID_SIZE = _game.terrain.terrainSize / 8.0f;
            
            _cities.Clear();

            Random random = new (BMath.Max(1, (uint)_game.terrain.seed));
            
            int cityCountX = BMath.CeilToInt(_game.terrain.terrainSize / CITY_GRID_SIZE);
            float collisionRadius = CITY_GRID_SIZE * 0.5f;

            FloatBounds2D globalBounds = default;

            for (int y = 0; y < cityCountX; y++)
            {
                FloatBounds2D localBounds = default;
                
                for (int x = 0; x < cityCountX; x++)
                {
                    float2 cityCenter = new float2(x, y) * CITY_GRID_SIZE + new float2(CITY_GRID_SIZE * 0.5f);
                    float2 randomXY =  random.NextFloat2() * 2.0f - 1.0f;
                    cityCenter += randomXY * CITY_GRID_SIZE * 0.5f;

                    cityCenter = BMath.FloorToInt(cityCenter) + new float2(0.5f);

                    if (localBounds.Contains(cityCenter) || globalBounds.Contains(cityCenter))
                    {
                        continue;
                    }

                    float2 orgPosition = cityCenter;

                    TryCorrectCityCenter(ref cityCenter, out ECityStatus status);

                    localBounds.x.max = BMath.Max(cityCenter.x + collisionRadius, localBounds.x.max);
                    localBounds.y.max = BMath.Max(cityCenter.y + collisionRadius, localBounds.y.max);
                    
                    _cities.Add(new CityInfo
                    {
                        originalPosition = orgPosition,
                        correctedPosition = cityCenter,
                        status = status
                    });       
                }

                globalBounds.x.max = BMath.Max(localBounds.x.max, globalBounds.x.max);
                globalBounds.y.max = BMath.Max(localBounds.y.max, globalBounds.y.max);
            }
            
            return UniTask.CompletedTask;
        }

        private void TryCorrectCityCenter(ref float2 center, out ECityStatus status)
        {
            int2 centerInt = BMath.FloorToInt(center);
            
            AveragePixelCountTexture<EBiomeName> terrain8km = _biome.Terrain8kmCell;
            FloatBounds2D mapBounds = new (0, _game.terrain.terrainSize - 1);

            if (_isValidBiomeForCity(_biome.Terrain1kmCell.GetPixelClamped(centerInt)))
            {
                // Try to move city closer to the ocean
                for (int radius = 0; radius < 15; radius++)
                {
                    CitySpawnPointSearch iterator = new (radius);
                    
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
            for (int radius = 0; radius < 15; radius++)
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
            int2 cellPosition = _biome.Terrain8kmCell.FloorToCellWorldPosition(referencePosition);
            int blockOffset = _biome.Terrain1kmCell.GetBlockOffset(cellPosition);
            int cellSize = _biome.Terrain8kmCell.cellSize;

            for (int i = 0; i < cellSize * cellSize; i++)
            {
                int2 pixelXY = new (i % cellSize, i / cellSize);

                int innerOffset = _biome.Terrain1kmCell.GetInnerOffset(cellPosition + pixelXY);

                EBiomeName biome = _biome.Terrain1kmCell.texture[blockOffset + innerOffset];

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

        private void _render(BlockingContext context)
        {
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

            BDebug.Track(this, "CityCount", _cities.Count);
        }
    }
}