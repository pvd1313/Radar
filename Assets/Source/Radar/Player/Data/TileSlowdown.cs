using System;
using Beast.Radar.Terrain;

namespace Beast.Radar.Player
{
    [Serializable]
    public struct TileSlowdown
    {
        public ETerrainBiome biome;
        
        public float speedMultiplier;
    }
}