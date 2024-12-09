namespace Beast.Radar.Terrain
{
    public struct TerrainCell
    {
        public ETerrainBiome Biome => (ETerrainBiome)(_value >> 4);
        public ETerrainFeature Feature => (ETerrainFeature)(_value & 0xF);
        
        private byte _value;
        
        public TerrainCell(ETerrainBiome biome, ETerrainFeature feature)
        {
            _value = (byte)((int)biome << 4 | ((int)feature & 0xF));
        }
    }
}