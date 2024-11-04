namespace Beast.Radar.Terrain
{
    public struct BiomeTextureConverter : IShortTextureConverter<EBiomeName>
    {
        public EBiomeName Convert(short pixel)
        {
            return (EBiomeName)pixel;
        }
    }
}