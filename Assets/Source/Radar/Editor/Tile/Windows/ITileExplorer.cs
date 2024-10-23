using Beast.Claw.Common;
using Beast.Radar.Terrain;

namespace Beast.Radar.Editor.Tile
{
    public interface ITileExplorer
    {
        ScriptableContainer<TerrainAtlas>  SelectedAsset { get; }
        
        TerrainTileData SelectedTile { get; }
    }
}