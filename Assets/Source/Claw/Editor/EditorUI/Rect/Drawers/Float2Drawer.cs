using System;
using UnityEngine;

namespace Beast.Claw.Editor.Drawers
{
    public class Float2Drawer : IRectDrawer<Float2Task>
    {
        private RectUI _ui;

        public void Warmup(RectUI ui)
        {
            _ui = ui;
        }

        public void Draw(Rect screen, Float2Task task)
        {
            if (task.mainLabel.hasLabel)
            {
                screen = _ui.PrefixLabel(screen, task.mainLabel, task.mainLabelStyle);
            }

            float label0 = task.label0.CalcSizeOrDefault(task.labelStyle0).x;
            float label1 = task.label1.CalcSizeOrDefault(task.labelStyle1).x;
            float label2 = task.label2.CalcSizeOrDefault(task.labelStyle2).x;
            float field0 = (screen.width - label0 - label1 - label2) * 0.5f;
            float field1 = field0;
            float afterField0 = 0;
            float afterField1 = 0;

            if (label0 > 0)
            {
                label0 += task.fieldLabelSpacing;
                field0 -= task.fieldLabelSpacing;
            }
            
            if (label1 > 0)
            {
                label1 += task.fieldLabelSpacing;
                field1 -= task.fieldLabelSpacing;
                afterField0 += task.fieldLabelSpacing;
                field0 -= task.fieldLabelSpacing;
            }

            if (label2 > 0)
            {
                afterField1 += task.fieldLabelSpacing;
                field1 -= task.fieldLabelSpacing;
            }

            RectSlice columns = new (stackalloc Rect[7]);
            columns.Columns(screen, label0, field0, afterField0, label1, field1, afterField1, label2);

            using IDisposable _ = _ui.RemoveIndent();

            if (label0 > 0)
            {
                _ui.Label(columns[0], task.label0, task.labelStyle0);
            }

            task.value0 = _ui.FloatField(columns[1], task.value0, task.valueStyle0);

            if (label1 > 0)
            {
                _ui.Label(columns[3], task.label1, task.labelStyle1);
            }

            if (label2 > 0)
            {
                _ui.Label(columns[6], task.label2, task.labelStyle2);
            }

            task.value1 = _ui.FloatField(columns[4], task.value1, task.valueStyle1);
        }
    }
}