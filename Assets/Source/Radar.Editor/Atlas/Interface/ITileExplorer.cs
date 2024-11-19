using Beast.Claw.Common;
using Beast.Radar.Terrain;

namespace Beast.Radar.Editor.Atlas
{
    public interface ITileExplorer
    {
        ScriptableContainer<TerrainAtlas> SelectedAsset { get; }
        
        TerrainTileData SelectedTile { get; }
    }
}