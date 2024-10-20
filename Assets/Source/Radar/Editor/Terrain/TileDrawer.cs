using Beast.Claw.Editor;
using UnityEditor;
using UnityEngine;

namespace Beast.Radar.Editor.Terrain
{
    public class TileDrawer : IRectDrawer<TileDrawerTask>
    {
        private GUIStyle _tileNameStyle;
        private GUIStyle _emptyRectStyle;
        private RectUI _ui;

        public void Initialize(RectUI ui)
        {
            _ui = ui;
            
            _tileNameStyle = new GUIStyle(EditorStyles.helpBox);
            _tileNameStyle.wordWrap = false;
            
            _emptyRectStyle = new GUIStyle();
        }

        public void Draw(Rect screen, TileDrawerTask task)
        {
            (Rect itemNameRect, _) = screen.SliceToRows2(16, screen.height - 16);
            
            _ui.Texture(screen, task.tile.texture);
            
            if (screen.width >= 32)
            {
                _ui.Fill(itemNameRect, UIStyle.EditorSkinBackgroundColor);
                
                Label tileName = task.tile.name;

                if (_tileNameStyle.CalcSize(tileName.content).x > screen.width)
                {
                    _tileNameStyle.alignment = TextAnchor.MiddleRight;
                }
                else
                {
                    _tileNameStyle.alignment = TextAnchor.MiddleCenter;
                }
                
                _ui.Label(itemNameRect, task.tile.name, _tileNameStyle);
            }
        }
    }
}