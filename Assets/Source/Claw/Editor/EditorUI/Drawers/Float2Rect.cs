using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor.Drawers
{
    public class Float2Rect : IRectDrawer<Float2Task>
    {
        private RectUI _ui;

        public void Initialize(RectUI ui)
        {
            _ui = ui;
        }

        public void Draw(Rect screen, Float2Task task)
        {
            if (task.mainLabel.hasLabel)
            {
                screen = _ui.PrefixLabel(screen, task.mainLabel, task.mainLabelStyle);
            }

            float labelWidth0 = task.label0.CalcSizeOrDefault(task.labelStyle0).x;
            float labelWidth1 = task.label1.CalcSizeOrDefault(task.labelStyle1).x;
            float fieldWidth = (screen.width - labelWidth0 - labelWidth1) * 0.5f;

            Rect4 columns = screen.SliceToColumns4(labelWidth0, fieldWidth, labelWidth1, fieldWidth);
            
            // This probably should be handled by RectUI.
            int indentLevelBak = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            if (labelWidth0 > 0)
            {
                _ui.PrefixLabel(columns.rect0, task.label0, task.labelStyle0);
            }

            task.value0 = _ui.FloatField(columns.rect1, task.value0, task.labelStyle0);

            if (labelWidth1 > 0)
            {
                _ui.PrefixLabel(columns.rect2, task.label1, task.labelStyle1);
            }

            task.value1 = _ui.FloatField(columns.rect3, task.value1, task.labelStyle1);

            EditorGUI.indentLevel = indentLevelBak;
        }
    }
}