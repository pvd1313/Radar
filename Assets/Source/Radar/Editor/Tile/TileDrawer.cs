using Beast.Claw.Editor;
using UnityEditor;
using UnityEngine;

namespace Beast.Radar.Editor.Tile
{
    public class TileDrawer : IRectDrawer<TileDrawerTask>
    {
        private GUIStyle _tileNameStyleNormal;
        private GUIStyle _tileNameStyleSelected;
        private GUIStyle _boxStyleNormal;
        private GUIStyle _boxStyleSelected;
        private RectUI _ui;

        public void Initialize(RectUI ui)
        {
            _ui = ui;
            
            _tileNameStyleNormal = new GUIStyle(EditorStyles.helpBox);
            _tileNameStyleNormal.wordWrap = false;

            _tileNameStyleSelected = new GUIStyle(_tileNameStyleNormal);
            _tileNameStyleSelected.normal.textColor = Color.yellow;
            _tileNameStyleSelected.wordWrap = false;
            
            _boxStyleNormal = new GUIStyle();
            _boxStyleSelected = EditorStyles.selectionRect;
        }

        public void Draw(Rect screen, TileDrawerTask task)
        {
            (Rect itemNameRect, _) = screen.SliceToRows2(16, screen.height - 16);
            
            Rect texture = screen.Padding(new Padding(2, 2, 0, 2));

            if (task.tile.texture != null)
            {
                _ui.Texture(texture, task.tile.texture);
            }
            else
            {
                _ui.Fill(screen, Color.black);
            }
            
            _ui.Fill(itemNameRect, UIStyle.EditorSkinBackgroundColor);

            Label tileNameLabel = !string.IsNullOrEmpty(task.tile.name) ? task.tile.name : "???";
            
            bool isSelected = task.selectedIndex == task.tileIndex;

            Style tileNameStyle = isSelected ? _tileNameStyleSelected : _tileNameStyleNormal; 

            if (tileNameStyle.style.CalcSize(tileNameLabel.content).x > screen.width)
            {
                tileNameStyle.style.alignment = TextAnchor.MiddleRight;
            }
            else
            {
                tileNameStyle.style.alignment = TextAnchor.MiddleCenter;
            }
                
            _ui.Label(itemNameRect, tileNameLabel, tileNameStyle);
            
            Style boxStyle = isSelected ? _boxStyleSelected : _boxStyleNormal;
            Rect boxScreen = screen.Padding(new Padding(-4, -4, -4, -2));
            
            if (_ui.Button(boxScreen, Label.NoLabel, boxStyle))
            {
                task.selectedIndex = task.tileIndex;
            }
        }
    }
}