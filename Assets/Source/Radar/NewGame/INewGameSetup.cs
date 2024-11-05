using Beast.Radar.Terrain;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.NewGame
{
    public interface INewGameSetup
    {
        UniTask StartNewGame(TerrainData terrain);
    }
}