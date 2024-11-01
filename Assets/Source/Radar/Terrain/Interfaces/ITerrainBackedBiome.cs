using Beast.Claw.Common;

namespace Beast.Radar.Terrain
{
    public interface ITerrainBackedBiome : IReady
    {
        TextureWithBlocks Terrain1kmCell { get; }
    }
}