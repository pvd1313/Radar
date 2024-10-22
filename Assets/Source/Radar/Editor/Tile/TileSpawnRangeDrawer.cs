using Beast.Claw.Editor;
using Beast.Claw.Editor.Drawers;
using Beast.Claw.Math;
using UnityEngine;

namespace Beast.Radar.Editor.Tile
{
    public class TileSpawnRangeDrawer : IFieldDrawer<FloatMinMax>
    {
        private readonly Color _textColor;

        private Style _fieldStyle;
        private string _leftLabel;
        
        private RectUI _ui;

        public TileSpawnRangeDrawer(Color textColor)
        {
            _textColor = textColor;
        }

        public void Warmup(RectUI ui)
        {
            _ui = ui;
            GUIStyle fieldStyle = new (ui.Style.Label.style);
            fieldStyle.normal.textColor = _textColor;
            fieldStyle.alignment = TextAnchor.MiddleCenter;
            
            _fieldStyle = fieldStyle;
        }

        public FloatMinMax Draw(FieldTask<FloatMinMax> field)
        {
            _ui.TempTaskWithDefaultStyle(out Float2Task task);

            task.fieldLabelSpacing = 0;
            task.valueStyle0 = _fieldStyle;
            task.valueStyle1 = _fieldStyle;
            task.label0 = "[";
            task.label1 = ",";
            task.label2 = "]";
            task.value0 = field.value.min;
            task.value1 = field.value.max;
            
            _ui.Fill(field.screen, Color.black);
            
            _ui.Draw(field.screen, task);

            return new FloatMinMax(task.value0, task.value1);
        }
    }
}