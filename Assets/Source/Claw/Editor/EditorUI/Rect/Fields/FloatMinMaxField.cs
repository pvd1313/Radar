using Beast.Claw.Editor.Drawers;
using Beast.Claw.Mathematics;

namespace Beast.Claw.Editor.Fields
{
    public class FloatMinMaxField : IFieldDrawer<FloatMinMax>
    {
        private RectUI _ui;

        public void Warmup(RectUI ui, UIStyleGroup style)
        {
            _ui = ui;
        }

        public FloatMinMax Draw(FieldTask<FloatMinMax> field)
        {
            _ui.TempTaskWithDefaultStyle(out Float2Task task);

            task.mainLabel = field.label;
            task.label0 = "min";
            task.label1 = "max";
            task.value0 = field.value.min;
            task.value1 = field.value.max;

            _ui.Draw(field.screen, task);

            return new FloatMinMax(task.value0, task.value1);
        }
    }
}