using System;
using Beast.Radar.Terrain;

namespace Beast.Radar.Player
{
    [Serializable]
    public struct TileSlowdown
    {
        public EBiomeName biome;
        
        public float speedMultiplier;
    }
}