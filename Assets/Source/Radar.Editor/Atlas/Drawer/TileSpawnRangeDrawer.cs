using Beast.Claw.Editor;
using Beast.Claw.Editor.UI;
using Beast.Claw.Editor.UI.UI;
using Beast.Claw.Math;
using UnityEngine;

namespace Beast.Radar.Editor.Atlas
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

        public void Warmup(RectUI ui, UIStyleGroup style)
        {
            _ui = ui;
            
            _fieldStyle = ui.Style.Label.Clone();
            _fieldStyle.Scheme.normal.textColor = _textColor;
            _fieldStyle.Scheme.alignment = TextAnchor.MiddleCenter;
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