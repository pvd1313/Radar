namespace Beast.Radar.Terrain
{
    public struct BiomeTextureConverter : IShortTextureConverter<TerrainCell>
    {
        public TerrainCell Convert(short pixel)
        {
            return new TerrainCell((ETerrainBiome)pixel, ETerrainFeature.None);
        }
    }
}