using Beast.Claw;
using Beast.Radar.Terrain;

namespace Beast.Radar.Editor.Terrain
{
    public class TileDrawerTask : IValidatable
    {
        public int selectedIndex;
        public int tileIndex;
        public TerrainTileData tile;
            
        public string Validate()
        {
            if (tileIndex < 0)
            {
                return "Invalid tile index: " + tileIndex;
            }

            if (tile == null)
            {
                return "Tile is null";
            }

            return null;
        }
    }
}