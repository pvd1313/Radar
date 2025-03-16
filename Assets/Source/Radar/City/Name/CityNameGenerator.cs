using System.Collections.Generic;
using Beast.Claw.Attributes;
using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Radar.Terrain;
using Beast.Radar.World;
using Cysharp.Threading.Tasks;
using Unity.Collections;

namespace Beast.Radar.City
{
    public class CityNameGenerator : Actor, ICityNameGenerator
    {
        [Required] private readonly CityNameConfig _config;
        [Required] private readonly IWorldManager _worldManager;
        [Required] private readonly ICitySpawner _citySpawner;
        
        private string[] _cityNames;

        private TerrainData _bakedForTerrain;

        protected override UniTask _gameLoad(ConcurrentContext context)
        {
            TerrainData terrain = _worldManager.GetCurrentWorld().terrain;
            if (terrain.IsSame(_bakedForTerrain))
            {
                return UniTask.CompletedTask;
            }

            _bakedForTerrain = terrain;
            NativeSlice<CityInfo> cities = _citySpawner.Cities;
            _cityNames = new string[cities.Length];

            int seed = _bakedForTerrain.seed;
            
            List<string> majorNames = new(_config.majorNames);
            majorNames.Shuffle(seed);
            int majorIndex = 0;
            
            List<string> minorPart0 = new(_config.minorNamePart0);
            minorPart0.Shuffle(seed + 1);
            int minorIndex0 = 0;
            
            List<string> minorPart1 = new(_config.minorNamePart1);
            minorPart1.Shuffle(seed + 2);
            int minorIndex1 = 0;

            for (int i = 0; i < cities.Length; i++)
            {
                CityInfo city = cities[i];

                if (city.IsMajor())
                {
                    _cityNames[i] = majorNames[majorIndex % majorNames.Count];
                    majorIndex++;
                }
                else
                {
                    string minor0 = minorPart0[(minorIndex0 / minorPart0.Count + minorIndex0) % minorPart0.Count];
                    string minor1 = minorPart1[minorIndex1 % minorPart1.Count];

                    _cityNames[i] = minor0 + minor1;

                    minorIndex0++;
                    minorIndex1++;
                }
            }
            
            return UniTask.CompletedTask;
        }

        public string GetCityName(int cityID)
        {
            return _cityNames[cityID];
        }
    }
}