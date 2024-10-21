using Beast.Radar.Terrain;
using UnityEngine;

namespace Beast.Radar.Editor.Tile
{
    public interface ITileExplorer
    {
        Object SelectedAsset { get; }
        
        TerrainTileData SelectedTile { get; }
    }
}