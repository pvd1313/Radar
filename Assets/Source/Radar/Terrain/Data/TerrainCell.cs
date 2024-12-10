using System.Runtime.CompilerServices;
using Beast.Claw.Math;

namespace Beast.Radar.Terrain
{
    public readonly struct TerrainCell
    {
        public ETerrainBiome Biome => (ETerrainBiome)(_value >> 4);
        public ETerrainFeature Feature => _extractFeature();
        
        private readonly byte _value;
        
        public TerrainCell(ETerrainBiome biome, ETerrainFeature feature)
        {
            _value = (byte)(((int)biome << 4) | ((int)feature & 0xF));
        }

        public byte GetFeatureMinCost(ByteArray16 cost)
        {
            int featureMask = (int)_extractFeature();

            if (featureMask == 0)
            {
                return 255;
            }

            int minCost = 255;

            if ((featureMask & 1) == 1)
            {
                minCost = BMath.Min(minCost, cost.GetByte(0));
            }

            if ((featureMask & 2) == 2)
            {
                minCost = BMath.Min(minCost, cost.GetByte(1));
            }
            
            if ((featureMask & 4) == 4)
            {
                minCost = BMath.Min(minCost, cost.GetByte(2));
            }
            
            if ((featureMask & 8) == 8)
            {
                minCost = BMath.Min(minCost, cost.GetByte(3));
            }

            return (byte)minCost;
        }

        public bool HasFeature(ETerrainFeature feature)
        {
            return ((int)_extractFeature() & (int)feature) == (int)feature;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ETerrainFeature _extractFeature()
        {
            return (ETerrainFeature)(_value & 0xF);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ETerrainBiome _extractBiome()
        {
            return (ETerrainBiome)(_value >> 4);
        }

        public TerrainCell AddFeature(ETerrainFeature feature)
        {
            int featureMask = (int)_extractFeature();
            featureMask |= (int)feature;

            return new TerrainCell(_extractBiome(), (ETerrainFeature)featureMask);
        }
    }
}