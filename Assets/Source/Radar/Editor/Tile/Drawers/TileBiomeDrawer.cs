using Beast.Claw.Editor;
using Beast.Claw.Mathematics;
using Sirenix.Utilities;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Editor.Tile
{
    public class TileBiomeDrawer : IRectDrawer<TileBiomeTask>
    {
        private RectUI _ui;
        private Style _labelStyleNormal;
        private Style _labelStyleSelected;

        public void Warmup(RectUI ui, UIStyleGroup style)
        {
            _ui = ui;
            
            _labelStyleNormal = style.Label.Clone();
            _labelStyleNormal.Scheme.wordWrap = false;
            _labelStyleNormal.Scheme.normal.textColor = Color.white;

            _labelStyleSelected = style.Label.Clone();
            _labelStyleSelected.Scheme.wordWrap = false;
            _labelStyleSelected.Scheme.normal.textColor = Color.yellow;
        }

        public void Draw(Rect screen, TileBiomeTask task)
        {
            switch (task.mode)
            {
                case TileBiomeTask.Mode.Background:
                    _ui.Fill(screen, task.tile.averageColor);
                    return;
                
                case TileBiomeTask.Mode.Label:
                    Label tileName = task.tile.name ?? "???";
                    Style labelStyle = task.isSelected ? _labelStyleSelected : _labelStyleNormal;
                    float2 labelSize = labelStyle.CalcSize(tileName);

                    Rect labelRect = screen.AlignCenter(labelSize);

                    // float xPadding = BMath.Max(0, labelSize.x - labelRect.width) * 0.5f;
                    // float yPadding = BMath.Max(0, labelSize.y - labelRect.height) * 0.5f;

                    //labelRect = labelRect.Expand(new float2(xPadding, yPadding));
            
                    _ui.Fill(labelRect, Color.black);
                    
                    // labelStyle.Scheme.alignment = 
                    //     labelRect.x > screen.width ? 
                    //     TextAnchor.MiddleLeft : 
                    //     TextAnchor.MiddleCenter;
                    
                    _ui.Label(labelRect, tileName, labelStyle);
                    return;
            }
        }
    }
}