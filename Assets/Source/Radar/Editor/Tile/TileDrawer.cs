using System;
using Beast.Claw.Editor;
using UnityEditor;
using UnityEngine;

namespace Beast.Radar.Editor.Tile
{
    public class TileDrawer : IRectDrawer<TileTask>
    {
        private Style _tileNameNormal;
        private Style _tileNameSelected;
        private Style _boxNormal;
        private Style _boxSelected;
        private RectUI _ui;

        private TileSpawnRangeDrawer _waterDrawer;
        private TileSpawnRangeDrawer _forestDrawer;

        public void Warmup(RectUI ui)
        {
            _ui = ui;
            
            GUIStyle tileNameNormal = new (EditorStyles.numberField);
            tileNameNormal.wordWrap = false;
            
            GUIStyle tileNameSelected = new (tileNameNormal);
            tileNameSelected.normal.textColor = Color.yellow;

            _boxNormal = Style.NoStyle;
            _boxSelected = EditorStyles.selectionRect;
            _tileNameNormal = tileNameNormal;
            _tileNameSelected = tileNameSelected;
            
            _waterDrawer = new TileSpawnRangeDrawer(Color.cyan);
            _waterDrawer.Warmup(_ui);
            
            _forestDrawer = new TileSpawnRangeDrawer(Color.green);
            _forestDrawer.Warmup(_ui);
        }

        public void Draw(Rect screen, TileTask task)
        {
            bool isSelected = task.selectedIndex == task.tileIndex;
            Style tileNameStyle = isSelected ? _tileNameSelected : _tileNameNormal; 
            
            Label tileNameLabel = !string.IsNullOrEmpty(task.tile.name) ? task.tile.name : "???";

            float labelHeight = tileNameStyle.CalcSize(tileNameLabel).y;
            (Rect itemNameRect, _) = screen.SliceToRows(labelHeight, screen.height - labelHeight);
            
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

            if (tileNameStyle.CalcSize(tileNameLabel).x > screen.width)
            {
                tileNameStyle.style.alignment = TextAnchor.MiddleRight;
            }
            else
            {
                tileNameStyle.style.alignment = TextAnchor.MiddleCenter;
            }

            _ui.Label(itemNameRect, tileNameLabel, tileNameStyle);

            if (task.displaySpawnRange)
            {
                Label water = "W";

                labelHeight = water.CalcSize(EditorStyles.numberField).y;

                RectSlice slice = new (stackalloc Rect[3]);
                slice.Rows(screen, screen.height - labelHeight * 2 - 2, labelHeight, labelHeight);

                using (IDisposable _ = _ui.RemoveInteraction())
                {
                    const string FORMAT = "[0.99,0.99]";
                    
                    Rect label = slice[1];
                    label.width = _ui.Style.Label.CalcSize(FORMAT).x;
                    label.x += 2;

                    if (label.width <= screen.width)
                    {
                        _ui.CustomField(label, Label.NoLabel, task.tile.waterRange, _waterDrawer);
                    
                        label = slice[2];
                        label.width = _ui.Style.Label.CalcSize(FORMAT).x;
                        label.x += 2;
                        _ui.CustomField(label, Label.NoLabel, task.tile.forestRange, _forestDrawer);
                    }
                }
            }
            
            Style boxStyle = isSelected ? _boxSelected : _boxNormal;
            Rect boxScreen = screen.Padding(new Padding(-4, -4, -4, -2));
            
            if (_ui.Button(boxScreen, Label.NoLabel, boxStyle))
            {
                task.selectedIndex = task.tileIndex;
            }
        }
    }
}