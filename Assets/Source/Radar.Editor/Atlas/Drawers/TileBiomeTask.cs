using Beast.Claw.Editor;
using Beast.Radar.Terrain;

namespace Beast.Radar.Editor.Tile
{
    public class TileBiomeTask : IRectDrawTask
    {
        public enum Mode
        {
            None,
            Label,
            Background,
            Interactive
        }
        
        public TerrainTileData tile;
        public bool isSelected;
        public Mode mode;
        
        public string Validate()
        {
            if (tile == null)
            {
                return "tile is null";
            }
            
            return null;
        }

        public void SetStyle(UIStyleGroup style)
        {
            
        }
    }
}