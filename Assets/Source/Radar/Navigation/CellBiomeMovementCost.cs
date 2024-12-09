using System;
using Beast.Radar.Terrain;

namespace Beast.Radar.Navigation
{
    [Serializable]
    public struct CellBiomeMovementCost
    {
        public ETerrainBiome biome;
        public byte cost;
    }
}