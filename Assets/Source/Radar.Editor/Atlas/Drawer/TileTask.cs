using Beast.Claw.Editor;
using Beast.Claw.Editor.UI;
using Beast.Radar.Terrain;

namespace Beast.Radar.Editor.Atlas
{
    public class TileTask : IRectDrawTask
    {
        public int selectedIndex;
        public int tileIndex;
        public TerrainTileData tile;
        public bool displaySpawnRange;
        
        public void SetStyle(UIStyleGroup style)
        {
            
        }
            
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