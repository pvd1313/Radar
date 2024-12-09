using Beast.Claw.Common;

namespace Beast.Radar.Terrain
{
    public interface ITerrainBackedBiome : IReady
    {
        TextureWithBlocks<TerrainCell> Terrain1kmCell { get; }
        AveragePixelCountTexture<TerrainCell> Terrain8kmCell { get; }
    }
}