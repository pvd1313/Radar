using Beast.Claw.Common;

namespace Beast.Radar.Terrain
{
    public interface ITerrainBackedBiome : IReady
    {
        TextureWithBlocks<EBiomeName> Terrain1kmCell { get; }
        AveragePixelCountTexture<EBiomeName> Terrain8kmCell { get; }
    }
}