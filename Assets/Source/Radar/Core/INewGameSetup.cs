using Beast.Radar.Terrain;

namespace Beast.Radar.Core
{
    public interface INewGameSetup
    {
        void StartNewGame(TerrainData terrain);
    }
}