using Beast.Claw.Common;

namespace Beast.Radar.Terrain
{
    public interface ITerrainBiomePostProcessor : IController
    {
        void Render(TerrainBiomePostProcess task);
    }
}